using MetalMetrics.Core.Entities;
using MetalMetrics.Core.Enums;
using MetalMetrics.Core.Interfaces;
using MetalMetrics.Infrastructure.Data;
using MetalMetrics.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantProvider, TenantProvider>();
builder.Services.AddScoped<IJobService, JobService>();
builder.Services.AddScoped<IQuoteService, QuoteService>();
builder.Services.AddScoped<IActualsService, ActualsService>();
builder.Services.AddScoped<IProfitabilityService, ProfitabilityService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IReportsService, ReportsService>();
builder.Services.AddHttpClient("ClaudeAI");
builder.Services.AddScoped<IAIQuoteService, ClaudeAIQuoteService>();
builder.Services.AddScoped<IJobAssignmentService, JobAssignmentService>();
builder.Services.AddScoped<ITimeEntryService, TimeEntryService>();
builder.Services.AddScoped<IJobNoteService, JobNoteService>();
builder.Services.AddScoped<IEmailService, SendGridEmailService>();
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
builder.Services.AddScoped<IPlatformService, PlatformService>();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Stripe configuration
var stripeSecretKey = builder.Configuration["Stripe:SecretKey"];
if (!string.IsNullOrEmpty(stripeSecretKey))
{
    Stripe.StripeConfiguration.ApiKey = stripeSecretKey;
}

builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Login";
    options.AccessDeniedPath = "/AccessDenied";
    options.LogoutPath = "/Logout";
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", p => p.RequireRole("Admin", "Owner"));
    options.AddPolicy("CanManageJobs", p => p.RequireRole("Admin", "Owner", "ProjectManager", "Foreman"));
    options.AddPolicy("CanQuote", p => p.RequireRole("Admin", "Owner", "ProjectManager", "Estimator"));
    options.AddPolicy("CanEnterActuals", p => p.RequireRole("Admin", "Owner", "ProjectManager", "Foreman"));
    options.AddPolicy("CanViewReports", p => p.RequireRole("Admin", "Owner", "ProjectManager"));
    options.AddPolicy("CanAssignJobs", p => p.RequireRole("Admin", "Owner", "ProjectManager", "Foreman"));
    options.AddPolicy("PlatformAdmin", p => p.RequireRole("SuperAdmin"));
});

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Platform", "PlatformAdmin");
})
.AddMvcOptions(options =>
{
    options.Filters.Add<MetalMetrics.Web.Filters.SubscriptionPageFilter>();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/Error/{0}");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

// Stripe webhook endpoint
app.MapPost("/api/stripe/webhook", async (HttpContext context, ISubscriptionService subscriptionService) =>
{
    using var reader = new StreamReader(context.Request.Body);
    var json = await reader.ReadToEndAsync();
    var signature = context.Request.Headers["Stripe-Signature"].ToString();

    try
    {
        await subscriptionService.HandleStripeWebhookAsync(json, signature);
        return Results.Ok();
    }
    catch
    {
        return Results.BadRequest();
    }
}).AllowAnonymous();

// Apply migrations and seed data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    foreach (var role in Enum.GetNames<AppRole>())
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    // Seed SuperAdmin and plans in ALL environments
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
    var seeder = new DbSeeder(db, userManager, app.Environment.WebRootPath);
    await seeder.SeedSuperAdminAsync();

    if (app.Environment.IsDevelopment())
    {
        await seeder.SeedAsync();
    }
}

app.Run();
