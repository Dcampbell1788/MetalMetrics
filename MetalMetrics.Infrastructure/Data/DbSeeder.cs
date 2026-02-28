using MetalMetrics.Core.Entities;
using MetalMetrics.Core.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MetalMetrics.Infrastructure.Data;

public class DbSeeder
{
    private readonly AppDbContext _db;
    private readonly UserManager<AppUser> _userManager;
    private readonly string? _webRootPath;
    private readonly Random _rng = new(42); // fixed seed for reproducibility

    public DbSeeder(AppDbContext db, UserManager<AppUser> userManager, string? webRootPath = null)
    {
        _db = db;
        _userManager = userManager;
        _webRootPath = webRootPath;
    }

    public async Task SeedAsync()
    {
        if (await _db.Jobs.AnyAsync()) return; // idempotent

        var precisionRoster = new (string FullName, string Email, AppRole Role)[]
        {
            ("Mike Sandoval",    "mike@precisionmetal.demo",   AppRole.Owner),
            ("Karen Yates",      "karen@precisionmetal.demo",  AppRole.Admin),
            ("Dave Holbrook",    "dave@precisionmetal.demo",   AppRole.ProjectManager),
            ("Lisa Tran",        "lisa@precisionmetal.demo",   AppRole.ProjectManager),
            ("Rich Koenig",      "rich@precisionmetal.demo",   AppRole.Estimator),
            ("Tony Vasquez",     "tony@precisionmetal.demo",   AppRole.Foreman),
            ("Brenda Marsh",     "brenda@precisionmetal.demo", AppRole.Foreman),
            ("Jake Novak",       "jake@precisionmetal.demo",   AppRole.Journeyman),
            ("Sam Reeves",       "sam@precisionmetal.demo",    AppRole.Journeyman),
            ("Cody Whitfield",   "cody@precisionmetal.demo",   AppRole.Journeyman),
        };

        var budgetRoster = new (string FullName, string Email, AppRole Role)[]
        {
            ("Daryl Huff",   "daryl@budgetfab.demo",  AppRole.Owner),
            ("Janet Flores", "janet@budgetfab.demo",  AppRole.Admin),
            ("Greg Olsen",   "greg@budgetfab.demo",   AppRole.ProjectManager),
            ("Megan Cruz",   "megan@budgetfab.demo",  AppRole.Estimator),
            ("Ray Caldwell", "ray@budgetfab.demo",    AppRole.Foreman),
            ("Tyler Banks",  "tyler@budgetfab.demo",  AppRole.Journeyman),
            ("Nick Pereira", "nick@budgetfab.demo",   AppRole.Journeyman),
        };

        await SeedTenantAsync("Precision Metal Works", precisionRoster, profitable: true);
        await SeedTenantAsync("Budget Fabricators", budgetRoster, profitable: false);
    }

    private async Task SeedTenantAsync(
        string companyName,
        (string FullName, string Email, AppRole Role)[] roster,
        bool profitable)
    {
        var tenant = new Tenant
        {
            CompanyName = companyName,
        };
        tenant.TenantId = tenant.Id;
        _db.Tenants.Add(tenant);

        var settings = new TenantSettings
        {
            TenantId = tenant.Id,
            DefaultLaborRate = 85m,
            DefaultMachineRate = 175m,
            DefaultOverheadPercent = 15m,
            TargetMarginPercent = 20m
        };
        _db.TenantSettings.Add(settings);

        // Create all employees from the roster
        string ownerEmail = roster.First(e => e.Role == AppRole.Owner).Email;
        var createdUsers = new Dictionary<string, AppUser>();
        foreach (var (fullName, email, role) in roster)
        {
            var user = new AppUser
            {
                UserName = email,
                Email = email,
                FullName = fullName,
                TenantId = tenant.Id,
                Role = role,
                EmailConfirmed = true
            };
            await _userManager.CreateAsync(user, "Demo123!");
            await _userManager.AddToRoleAsync(user, role.ToString());
            await _userManager.AddClaimAsync(user,
                new System.Security.Claims.Claim("TenantId", tenant.Id.ToString()));
            createdUsers[email] = user;
        }

        await _db.SaveChangesAsync();

        var customers = new[]
        {
            "ABC Fabrication", "Metro Industries", "Smith & Sons",
            "Global Steel Co", "Apex Manufacturing", "Northwest Metals",
            "Cascade Structural", "Pacific Rim Fab"
        };

        var now = DateTime.UtcNow;
        int jobCount = profitable ? 24 : 16;
        var laborRate = 85m;
        var machineRate = 175m;
        var overheadPct = 15m;

        for (int i = 0; i < jobCount; i++)
        {
            var customer = customers[_rng.Next(customers.Length)];

            // Spread jobs across the last 6 months for monthly revenue distribution
            int daysBack = (i * 180 / jobCount) + _rng.Next(0, 10);
            var createdAt = now.AddDays(-daysBack);

            var job = new Job
            {
                JobNumber = $"JOB-{(i + 1):D4}",
                CustomerName = customer,
                Description = GetDescription(i),
                TenantId = tenant.Id,
                CreatedAt = createdAt,
                UpdatedAt = createdAt
            };

            // Status distribution: most completed/invoiced, a few in progress and quoted
            if (i < jobCount - 4)
            {
                job.Status = i % 4 == 0 ? JobStatus.Invoiced : JobStatus.Completed;
                job.CompletedAt = createdAt.AddDays(_rng.Next(3, 14));
            }
            else if (i == jobCount - 3 || i == jobCount - 4)
            {
                job.Status = JobStatus.InProgress;
            }
            else
            {
                job.Status = JobStatus.Quoted;
            }

            _db.Jobs.Add(job);
            await _db.SaveChangesAsync();

            // Create estimate with realistic sheetmetal fabrication pricing
            var laborHours = RandDecimal(8, 80);
            var materialCost = RandDecimal(500, 12000);
            var machineHours = RandDecimal(2, 30);

            var laborCost = laborHours * laborRate;
            var machineCost = machineHours * machineRate;
            var subtotal = laborCost + materialCost + machineCost;
            var overhead = subtotal * (overheadPct / 100m);
            var totalEst = subtotal + overhead;

            // Set margins: profitable shop marks up more aggressively
            decimal marginMultiplier;
            if (profitable)
                marginMultiplier = 1.25m + (decimal)(_rng.NextDouble() * 0.25); // 25-50% margin
            else
                marginMultiplier = 1.10m + (decimal)(_rng.NextDouble() * 0.20); // 10-30% margin

            var quotePrice = Math.Round(totalEst * marginMultiplier, 2);

            var estimate = new JobEstimate
            {
                JobId = job.Id,
                TenantId = tenant.Id,
                EstimatedLaborHours = laborHours,
                LaborRate = laborRate,
                EstimatedMaterialCost = materialCost,
                EstimatedMachineHours = machineHours,
                MachineRate = machineRate,
                OverheadPercent = overheadPct,
                TotalEstimatedCost = totalEst,
                QuotePrice = quotePrice,
                EstimatedMarginPercent = quotePrice > 0 ? (quotePrice - totalEst) / quotePrice * 100m : 0,
                CreatedBy = ownerEmail,
                CreatedAt = createdAt,
                UpdatedAt = createdAt
            };
            _db.JobEstimates.Add(estimate);

            // Create actuals for completed/invoiced jobs
            if (job.Status == JobStatus.Completed || job.Status == JobStatus.Invoiced)
            {
                decimal varianceFactor;
                if (profitable)
                    varianceFactor = 0.88m + (decimal)(_rng.NextDouble() * 0.20); // -12% to +8% variance
                else
                    varianceFactor = 0.95m + (decimal)(_rng.NextDouble() * 0.45); // -5% to +40% variance

                // Make some jobs dramatically over budget for the struggling shop
                if (!profitable && i % 4 == 0)
                    varianceFactor = 1.25m + (decimal)(_rng.NextDouble() * 0.25); // 25-50% over

                var actLaborHours = Math.Round(laborHours * (0.85m + (decimal)(_rng.NextDouble() * 0.35)), 2);
                var actMaterialCost = Math.Round(materialCost * varianceFactor, 2);
                var actMachineHours = Math.Round(machineHours * (0.80m + (decimal)(_rng.NextDouble() * 0.40)), 2);

                var actLaborCost = actLaborHours * laborRate;
                var actMachineCost = actMachineHours * machineRate;
                var actSubtotal = actLaborCost + actMaterialCost + actMachineCost;
                var actOverhead = actSubtotal * (overheadPct / 100m);
                var actTotal = actSubtotal + actOverhead;

                var actRevenue = quotePrice; // invoice the quoted amount

                var actuals = new JobActuals
                {
                    JobId = job.Id,
                    TenantId = tenant.Id,
                    ActualLaborHours = actLaborHours,
                    LaborRate = laborRate,
                    ActualMaterialCost = actMaterialCost,
                    ActualMachineHours = actMachineHours,
                    MachineRate = machineRate,
                    OverheadPercent = overheadPct,
                    TotalActualCost = actTotal,
                    ActualRevenue = actRevenue,
                    EnteredBy = ownerEmail,
                    CreatedAt = job.CompletedAt ?? createdAt.AddDays(7),
                    UpdatedAt = job.CompletedAt ?? createdAt.AddDays(7)
                };
                _db.JobActuals.Add(actuals);
            }
        }

        await _db.SaveChangesAsync();

        // Seed job assignments, time entries, and notes
        var pms = createdUsers.Values.Where(u => u.Role == AppRole.ProjectManager).ToList();
        var estimators = createdUsers.Values.Where(u => u.Role == AppRole.Estimator).ToList();
        var foremen = createdUsers.Values.Where(u => u.Role == AppRole.Foreman).ToList();
        var journeymen = createdUsers.Values.Where(u => u.Role == AppRole.Journeyman).ToList();
        var owner = createdUsers.Values.First(u => u.Role == AppRole.Owner);

        var allJobs = await _db.Jobs.Where(j => j.TenantId == tenant.Id).ToListAsync();

        foreach (var job in allJobs)
        {
            // Assign PM(s) and Estimator(s) to all jobs
            foreach (var pm in pms)
            {
                _db.JobAssignments.Add(new JobAssignment
                {
                    JobId = job.Id, UserId = pm.Id, AssignedByUserId = owner.Id, TenantId = tenant.Id
                });
            }
            foreach (var est in estimators)
            {
                _db.JobAssignments.Add(new JobAssignment
                {
                    JobId = job.Id, UserId = est.Id, AssignedByUserId = owner.Id, TenantId = tenant.Id
                });
            }

            // Assign Foremen and Journeymen to InProgress/Completed/Invoiced jobs
            if (job.Status != JobStatus.Quoted)
            {
                foreach (var fm in foremen)
                {
                    _db.JobAssignments.Add(new JobAssignment
                    {
                        JobId = job.Id, UserId = fm.Id, AssignedByUserId = pms.First().Id, TenantId = tenant.Id
                    });
                }
                foreach (var jm in journeymen)
                {
                    _db.JobAssignments.Add(new JobAssignment
                    {
                        JobId = job.Id, UserId = jm.Id, AssignedByUserId = foremen.First().Id, TenantId = tenant.Id
                    });
                }
            }

            // Seed time entries for completed/invoiced jobs
            if (job.Status == JobStatus.Completed || job.Status == JobStatus.Invoiced)
            {
                var workers = foremen.Concat(journeymen).ToList();
                foreach (var worker in workers)
                {
                    int entryCount = _rng.Next(2, 5);
                    for (int d = 0; d < entryCount; d++)
                    {
                        _db.JobTimeEntries.Add(new JobTimeEntry
                        {
                            JobId = job.Id,
                            UserId = worker.Id,
                            HoursWorked = RandDecimal(2, 10),
                            WorkDate = job.CreatedAt.AddDays(d + 1),
                            TenantId = tenant.Id
                        });
                    }
                }
            }
        }

        // Seed a few sample notes on first 5 jobs
        var sampleJobs = allJobs.Take(5).ToList();
        var noteAuthors = foremen.Concat(journeymen).Take(3).ToList();
        var sampleNotes = new[]
        {
            "Material received and staged at work station.",
            "Laser cutting complete, moving to brake forming.",
            "Quality check passed. Ready for welding.",
            "Customer requested minor dimension change — adjusted.",
            "Finished deburring. Parts look good.",
            "Welding jig set up and first pieces tacked.",
            "Paint booth prepped. Powder coat scheduled for tomorrow.",
            "Dimensional check on first article — all within tolerance."
        };

        // Create placeholder images for some notes
        var imageLabels = new[]
        {
            "Material Staged", "Laser Cut Parts", "QC Passed",
            "Weld Setup", "Finished Parts"
        };
        var imageFileNames = new List<string>();
        if (_webRootPath != null)
        {
            var uploadsDir = Path.Combine(_webRootPath, "uploads", "notes");
            Directory.CreateDirectory(uploadsDir);
            foreach (var label in imageLabels)
            {
                var fileName = $"seed_{label.Replace(" ", "_").ToLower()}.svg";
                var filePath = Path.Combine(uploadsDir, fileName);
                if (!File.Exists(filePath))
                {
                    var svg = GeneratePlaceholderSvg(label);
                    await File.WriteAllTextAsync(filePath, svg);
                }
                imageFileNames.Add(fileName);
            }
        }

        int noteIdx = 0;
        foreach (var job in sampleJobs)
        {
            foreach (var author in noteAuthors)
            {
                string? imageFile = null;
                // Attach an image to roughly every other note
                if (imageFileNames.Count > 0 && noteIdx % 2 == 0)
                {
                    imageFile = imageFileNames[noteIdx / 2 % imageFileNames.Count];
                }

                _db.JobNotes.Add(new JobNote
                {
                    JobId = job.Id,
                    UserId = author.Id,
                    AuthorName = author.FullName,
                    Content = sampleNotes[noteIdx % sampleNotes.Length],
                    ImageFileName = imageFile,
                    TenantId = tenant.Id
                });
                noteIdx++;
            }
        }

        await _db.SaveChangesAsync();
    }

    private string GetDescription(int index)
    {
        var descriptions = new[]
        {
            "16ga mild steel brackets, qty 50, laser cut and brake formed",
            "Stainless steel enclosure panels, 14ga, welded assembly",
            "Aluminum mounting plates, 0.125\" thick, CNC punched",
            "Galvanized ductwork sections, 20ga, roll formed and seamed",
            "Stainless steel handrail components, tube and plate",
            "Mild steel structural gussets, 3/8\" plate, plasma cut",
            "Aluminum heat sink extrusion brackets, machined and deburred",
            "Copper bus bar connectors, precision sheared and drilled",
            "Mild steel tool cabinet frames, welded and powder coat ready",
            "Stainless surgical tray lids, mirror finish, laser cut",
            "Mild steel conveyor side rails, 10ga, bent and welded",
            "Aluminum electronic chassis, 0.060\" sheet, complex bends",
            "Galvanized HVAC transition pieces, custom layout",
            "Stainless kitchen hood panels, 18ga, welded corners",
            "Mild steel machine guards, expanded metal and frame",
            "Aluminum signage blanks, flat sheet, deburred edges",
            "Stainless wall cladding panels, #4 finish, drilled",
            "Mild steel pipe saddle supports, heavy plate",
            "Aluminum cable tray sections, perforated and bent",
            "Copper ground straps, precision cut and formed",
            "Steel stair stringer assemblies, 3/8\" plate, welded",
            "Stainless pharmaceutical tank baffles, electropolished",
            "Aluminum aerospace brackets, tight-tolerance CNC",
            "Mild steel loading dock bumper guards, 1/4\" plate"
        };
        return descriptions[index % descriptions.Length];
    }

    private decimal RandDecimal(decimal min, decimal max)
    {
        var range = (double)(max - min);
        return Math.Round(min + (decimal)(_rng.NextDouble() * range), 2);
    }

    private static string GeneratePlaceholderSvg(string label)
    {
        return $@"<svg xmlns=""http://www.w3.org/2000/svg"" width=""400"" height=""300"" viewBox=""0 0 400 300"">
  <rect width=""400"" height=""300"" fill=""#2c3e50""/>
  <rect x=""20"" y=""20"" width=""360"" height=""260"" rx=""8"" fill=""#34495e"" stroke=""#f39c12"" stroke-width=""2""/>
  <text x=""200"" y=""140"" text-anchor=""middle"" fill=""#f39c12"" font-family=""Arial,sans-serif"" font-size=""20"" font-weight=""bold"">{label}</text>
  <text x=""200"" y=""170"" text-anchor=""middle"" fill=""#95a5a6"" font-family=""Arial,sans-serif"" font-size=""14"">Sample shop photo</text>
  <circle cx=""200"" cy=""100"" r=""25"" fill=""none"" stroke=""#f39c12"" stroke-width=""2""/>
  <line x1=""200"" y1=""80"" x2=""200"" y2=""75"" stroke=""#f39c12"" stroke-width=""2""/>
  <circle cx=""200"" cy=""100"" r=""3"" fill=""#f39c12""/>
</svg>";
    }
}
