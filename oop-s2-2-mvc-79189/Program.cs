using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using oop_s2_2_mvc_79189.Data;
using Serilog;
using Serilog.Events;
using System;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .Enrich.WithProperty("Application", "FoodSafetyTracker")
    .WriteTo.Console()
    .WriteTo.File(
        path: "logs/log-.txt",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Application} {EnvironmentName} {UserName} {Message:lj}{NewLine}{Exception}"
    )
    .CreateBootstrapLogger();

try
{
    Log.Information("FoodSafetyTracker starting up");

    var builder = WebApplication.CreateBuilder(args);

    // Tell ASP.NET to use Serilog
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .MinimumLevel.Debug()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .Enrich.WithEnvironmentName()
        .Enrich.WithProperty("Application", "FoodSafetyTracker")
        .WriteTo.Console()
        .WriteTo.File(
            path: "logs/log-.txt",
            rollingInterval: RollingInterval.Day,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Application} {EnvironmentName} {Message:lj}{NewLine}{Exception}"
        ));



// database.
builder.Services.AddAuthorization();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//Identity configuration
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
    { options.SignIn.RequireConfirmedAccount = false;})
.AddRoles<IdentityRole>()          
.AddEntityFrameworkStores<ApplicationDbContext>();


//MVC configuration
builder.Services.AddControllersWithViews();
    var app = builder.Build();

    // ? Custom middleware must come FIRST before anything else
    app.UseMiddleware<oop_s2_2_mvc_79189.Middleware.ExceptionHandlingMiddleware>();

    // Remove the if/else block entirely and replace with just this:
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseSerilogRequestLogging(); // ? missing from yours
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Dashboard}/{action=Index}/{id?}");
    app.MapRazorPages();
    //Seed data
    using (var scope = app.Services.CreateScope())
{
    await DbSeeder.SeedAsync(scope.ServiceProvider);
}
app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}