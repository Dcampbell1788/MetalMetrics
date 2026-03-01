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

    // Realistic job profiles correlated by description.
    // Material costs target 40-55% of direct cost for most jobs.
    // (Description, LaborHrsMin, LaborHrsMax, MaterialMin, MaterialMax, MachineHrsMin, MachineHrsMax)
    private static readonly (string Desc, decimal LaborMin, decimal LaborMax,
        decimal MatMin, decimal MatMax, decimal MachMin, decimal MachMax)[] JobProfiles =
    {
        ("16ga mild steel brackets, qty 50, laser cut and brake formed",      12, 18,  1000,  1500,  3,  5),
        ("Stainless steel enclosure panels, 14ga, welded assembly",           40, 55,  5000,  7500,  8, 12),
        ("Aluminum mounting plates, 0.125\" thick, CNC punched",              8, 14,   900,  1400,  4,  7),
        ("Galvanized ductwork sections, 20ga, roll formed and seamed",       25, 35,  2500,  3500,  6, 10),
        ("Stainless steel handrail components, tube and plate",              35, 50,  4000,  6000,  5,  8),
        ("Mild steel structural gussets, 3/8\" plate, plasma cut",           10, 16,  2000,  3000,  5,  8),
        ("Aluminum heat sink extrusion brackets, machined and deburred",     15, 22,  1800,  2800,  8, 12),
        ("Copper bus bar connectors, precision sheared and drilled",          6, 10,  1800,  2800,  3,  5),
        ("Mild steel tool cabinet frames, welded and powder coat ready",     30, 45,  2500,  3800,  4,  6),
        ("Stainless surgical tray lids, mirror finish, laser cut",           20, 30,  3200,  4800,  6, 10),
        ("Mild steel conveyor side rails, 10ga, bent and welded",            25, 40,  2800,  4200,  6,  9),
        ("Aluminum electronic chassis, 0.060\" sheet, complex bends",        18, 28,  1200,  1800,  5,  8),
        ("Galvanized HVAC transition pieces, custom layout",                 20, 30,  1800,  2800,  4,  7),
        ("Stainless kitchen hood panels, 18ga, welded corners",              30, 45,  4500,  6500,  5,  8),
        ("Mild steel machine guards, expanded metal and frame",              15, 25,  1500,  2200,  3,  5),
        ("Aluminum signage blanks, flat sheet, deburred edges",               4,  8,   600,  1000,  2,  4),
        ("Stainless wall cladding panels, #4 finish, drilled",               20, 30,  4500,  6800,  4,  6),
        ("Mild steel pipe saddle supports, heavy plate",                     12, 20,  2200,  3400,  4,  6),
        ("Aluminum cable tray sections, perforated and bent",                15, 22,  1500,  2200,  5,  8),
        ("Copper ground straps, precision cut and formed",                    4,  8,  1800,  2800,  2,  4),
        ("Steel stair stringer assemblies, 3/8\" plate, welded",             45, 65,  5000,  7500,  8, 12),
        ("Stainless pharmaceutical tank baffles, electropolished",           35, 50,  6000,  9000,  6, 10),
        ("Aluminum aerospace brackets, tight-tolerance CNC",                 25, 40,  3000,  4800, 10, 16),
        ("Mild steel loading dock bumper guards, 1/4\" plate",               15, 25,  1800,  2800,  4,  6),
    };

    // Well-run shop attracts industrial/commercial customers
    private static readonly string[] PrecisionCustomers =
    {
        "Cascade Structural", "Pacific Rim Manufacturing", "Olympic Steel Systems",
        "Puget Sound Mechanical", "Evergreen Industrial", "Summit Engineering",
        "Columbia Precision", "Northwest Aerospace"
    };

    // Struggling shop takes whatever work walks in the door
    private static readonly string[] BudgetCustomers =
    {
        "ABC Fabrication", "Metro Industries", "Smith & Sons",
        "Quick Fix Mechanical", "Valley Sheet Metal", "Sunrise Construction"
    };

    public DbSeeder(AppDbContext db, UserManager<AppUser> userManager, string? webRootPath = null)
    {
        _db = db;
        _userManager = userManager;
        _webRootPath = webRootPath;
    }

    public async Task SeedSuperAdminAsync()
    {
        if (await _db.Users.AnyAsync(u => u.Role == AppRole.SuperAdmin)) return;

        var superAdmin = new AppUser
        {
            UserName = "admin@metalmetrics.io",
            Email = "admin@metalmetrics.io",
            FullName = "Platform Admin",
            TenantId = Guid.Empty,
            Role = AppRole.SuperAdmin,
            EmailConfirmed = true
        };
        await _userManager.CreateAsync(superAdmin, "SuperAdmin123!");
        await _userManager.AddToRoleAsync(superAdmin, AppRole.SuperAdmin.ToString());
        await _userManager.AddClaimAsync(superAdmin,
            new System.Security.Claims.Claim("TenantId", Guid.Empty.ToString()));
        await _userManager.AddClaimAsync(superAdmin,
            new System.Security.Claims.Claim("FullName", superAdmin.FullName));

        // Seed platform plans
        if (!await _db.PlatformPlans.AnyAsync())
        {
            _db.PlatformPlans.AddRange(
                new PlatformPlan
                {
                    Name = "Monthly",
                    Description = "Full access to MetalMetrics, billed monthly",
                    Interval = Core.Enums.PlanInterval.Monthly,
                    Price = 99m,
                    StripePriceId = "price_monthly_placeholder",
                    StripeProductId = "prod_placeholder",
                    TrialDays = 14
                },
                new PlatformPlan
                {
                    Name = "Annual",
                    Description = "Full access to MetalMetrics, billed annually (save 17%)",
                    Interval = Core.Enums.PlanInterval.Annual,
                    Price = 990m,
                    StripePriceId = "price_annual_placeholder",
                    StripeProductId = "prod_placeholder",
                    TrialDays = 14
                }
            );
            await _db.SaveChangesAsync();
        }
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
            SubscriptionStatus = Core.Enums.SubscriptionStatus.Active,
            IsEnabled = true,
            TrialEndsAt = DateTime.UtcNow.AddDays(-30), // trial already passed
        };
        tenant.TenantId = tenant.Id;
        _db.Tenants.Add(tenant);

        // Industry benchmarks:
        // Well-run shop: $95/hr labor, $175/hr machine, 18% overhead, targets 25% gross margin
        // Struggling shop: $68/hr labor (undercharges to win bids), $130/hr machine (older equipment),
        //   12% overhead (underestimates — real overhead is probably 20%+), targets 15% but rarely hits it
        var settings = new TenantSettings
        {
            TenantId = tenant.Id,
            DefaultLaborRate = profitable ? 95m : 68m,
            DefaultMachineRate = profitable ? 175m : 130m,
            DefaultOverheadPercent = profitable ? 30m : 20m,
            TargetMarginPercent = profitable ? 25m : 15m
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
            await _userManager.AddClaimAsync(user,
                new System.Security.Claims.Claim("FullName", fullName));
            createdUsers[email] = user;
        }

        await _db.SaveChangesAsync();

        var customers = profitable ? PrecisionCustomers : BudgetCustomers;

        var now = DateTime.UtcNow;
        int jobCount = profitable ? 24 : 16;
        var laborRate = settings.DefaultLaborRate;
        var machineRate = settings.DefaultMachineRate;
        var overheadPct = settings.DefaultOverheadPercent;

        for (int i = 0; i < jobCount; i++)
        {
            var profile = JobProfiles[i % JobProfiles.Length];
            var customer = customers[_rng.Next(customers.Length)];

            // Spread jobs across the last 6 months for monthly revenue distribution
            int daysBack = (i * 180 / jobCount) + _rng.Next(0, 10);
            var createdAt = now.AddDays(-daysBack);

            var job = new Job
            {
                JobNumber = $"JOB-{(i + 1):D4}",
                Slug = GenerateSlug(),
                CustomerName = customer,
                Description = profile.Desc,
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

            // Use profile-correlated costs instead of random ranges
            var laborHours = RandDecimal(profile.LaborMin, profile.LaborMax);
            var materialCost = RandDecimal(profile.MatMin, profile.MatMax);
            var machineHours = RandDecimal(profile.MachMin, profile.MachMax);

            var laborCost = laborHours * laborRate;
            var machineCost = machineHours * machineRate;
            var subtotal = laborCost + materialCost + machineCost;
            var overhead = subtotal * (overheadPct / 100m);
            var totalEst = subtotal + overhead;

            // Margin multipliers based on industry benchmarks:
            // Well-run shop: 25-35% gross margin (multiplier 1.30-1.55)
            // Struggling shop: 9-20% gross margin (multiplier 1.10-1.25) — undercharges to win bids
            decimal marginMultiplier;
            if (profitable)
                marginMultiplier = 1.30m + (decimal)(_rng.NextDouble() * 0.25);
            else
                marginMultiplier = 1.10m + (decimal)(_rng.NextDouble() * 0.15);

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

            // Create actuals for completed/invoiced jobs with per-component variance
            if (job.Status == JobStatus.Completed || job.Status == JobStatus.Invoiced)
            {
                decimal actLaborHours, actMaterialCost, actMachineHours;

                if (profitable)
                {
                    // Well-run shop: estimate vs actual variance ±5-10% (industry target)
                    // Good labor estimates, tight material control (low scrap), efficient machine use
                    actLaborHours = Math.Round(laborHours * (0.92m + (decimal)(_rng.NextDouble() * 0.16)), 2);     // 0.92-1.08
                    actMaterialCost = Math.Round(materialCost * (0.95m + (decimal)(_rng.NextDouble() * 0.10)), 2); // 0.95-1.05 (good nesting, 5% scrap)
                    actMachineHours = Math.Round(machineHours * (0.90m + (decimal)(_rng.NextDouble() * 0.15)), 2); // 0.90-1.05
                }
                else
                {
                    // Struggling shop: 20-30%+ estimate variance (industry reality without tracking)
                    // Labor always runs over (bad estimates), material waste from poor nesting (10-20% scrap)
                    actLaborHours = Math.Round(laborHours * (1.05m + (decimal)(_rng.NextDouble() * 0.35)), 2);     // 1.05-1.40 (always over)
                    actMaterialCost = Math.Round(materialCost * (0.95m + (decimal)(_rng.NextDouble() * 0.30)), 2); // 0.95-1.25 (waste/scrap)
                    actMachineHours = Math.Round(machineHours * (0.90m + (decimal)(_rng.NextDouble() * 0.25)), 2); // 0.90-1.15

                    // Every 3rd job has a major overrun: rework, scope creep, or blown estimate
                    if (i % 3 == 0)
                    {
                        actLaborHours = Math.Round(actLaborHours * 1.25m, 2);
                        actMaterialCost = Math.Round(actMaterialCost * 1.15m, 2);
                    }
                }

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

    private string GenerateSlug()
    {
        const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
        return new string(Enumerable.Range(0, 8)
            .Select(_ => chars[_rng.Next(chars.Length)])
            .ToArray());
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
