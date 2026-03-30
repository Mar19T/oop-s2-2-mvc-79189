using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using oop_s2_2_mvc_79189.Data;
using Serilog;
using System;

var builder = WebApplication.CreateBuilder(args);

// database.
builder.Services.AddAuthorization();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//Identity configuration
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
    { options.SignIn.RequireConfirmedAccount = false;})
.AddRoles<IdentityRole>()          
.AddEntityFrameworkStores<ApplicationDbContext>();

// Serilog configuration
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();
builder.Host.UseSerilog();

//MVC configuration
builder.Services.AddControllersWithViews();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
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
