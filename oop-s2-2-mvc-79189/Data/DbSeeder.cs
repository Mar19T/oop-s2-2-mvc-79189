using Inspections.Domain;
using Microsoft.AspNetCore.Identity;
using oop_s2_2_mvc_79189.Data;

namespace oop_s2_2_mvc_79189.Data
{
    public class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
            var db = services.GetRequiredService<ApplicationDbContext>();

            // ── Roles ──────────────────────────────────────────
            string[] roles = ["Admin", "Inspector", "Viewer"];
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // ── Users ──────────────────────────────────────────
            await CreateUser(userManager, "admin@food.ie",    "Admin123!", "Admin");
            await CreateUser(userManager, "inspector@food.ie","Inspect123!", "Inspector");
            await CreateUser(userManager, "viewer@food.ie",   "View123!",   "Viewer");

            // ── Premises ───────────────────────────────────────
            if (db.Premises.Any()) return; // don't seed twice

            var premises = new List<Premises>
            {
                // Dublin
                new() { Name = "The Greasy Spoon",    Address = "1 Main St",    Town = "Dublin", RiskRating = RiskRating.High },
                new() { Name = "Café Bella",           Address = "22 Grafton St", Town = "Dublin", RiskRating = RiskRating.Low },
                new() { Name = "Sunrise Bakery",       Address = "7 Talbot St",  Town = "Dublin", RiskRating = RiskRating.Medium },
                new() { Name = "Harbour Fish & Chips", Address = "3 Docklands",  Town = "Dublin", RiskRating = RiskRating.High },
                // Cork
                new() { Name = "The Olive Branch",    Address = "10 Patrick St", Town = "Cork",   RiskRating = RiskRating.Low },
                new() { Name = "Cork Spice House",    Address = "5 Union Quay",  Town = "Cork",   RiskRating = RiskRating.Medium },
                new() { Name = "Marina Diner",        Address = "18 Marina Rd",  Town = "Cork",   RiskRating = RiskRating.High },
                new() { Name = "The Baking Tin",      Address = "2 Washington St",Town = "Cork",  RiskRating = RiskRating.Low },
                // Galway
                new() { Name = "West Coast Bites",    Address = "9 Shop St",     Town = "Galway", RiskRating = RiskRating.Medium },
                new() { Name = "The Salt House",      Address = "4 Quay St",     Town = "Galway", RiskRating = RiskRating.Low },
                new() { Name = "Galway Grill House",  Address = "11 Eyre Sq",    Town = "Galway", RiskRating = RiskRating.High },
                new() { Name = "Connaught Kitchen",   Address = "6 Dominick St", Town = "Galway", RiskRating = RiskRating.Medium },
            };

            db.Premises.AddRange(premises);
            await db.SaveChangesAsync();

            // ── Inspections ────────────────────────────────────
            var now = DateTime.Today;
            var inspections = new List<Inspection>
            {
                // This month
                new() { PremisesId = premises[0].Id,  InspectionDate = now.AddDays(-2),  Score = 45, Outcome = Outcome.Fail, Notes = "Poor hygiene in kitchen." },
                new() { PremisesId = premises[1].Id,  InspectionDate = now.AddDays(-5),  Score = 88, Outcome = Outcome.Pass, Notes = "Well maintained." },
                new() { PremisesId = premises[2].Id,  InspectionDate = now.AddDays(-8),  Score = 55, Outcome = Outcome.Fail, Notes = "Storage issues found." },
                new() { PremisesId = premises[3].Id,  InspectionDate = now.AddDays(-10), Score = 72, Outcome = Outcome.Pass, Notes = "Generally good." },
                new() { PremisesId = premises[4].Id,  InspectionDate = now.AddDays(-3),  Score = 91, Outcome = Outcome.Pass, Notes = "Excellent standards." },
                new() { PremisesId = premises[5].Id,  InspectionDate = now.AddDays(-7),  Score = 60, Outcome = Outcome.Fail, Notes = "Temperature logs missing." },
                // Last month
                new() { PremisesId = premises[6].Id,  InspectionDate = now.AddDays(-35), Score = 40, Outcome = Outcome.Fail, Notes = "Rodent evidence found." },
                new() { PremisesId = premises[7].Id,  InspectionDate = now.AddDays(-40), Score = 78, Outcome = Outcome.Pass, Notes = "Good overall." },
                new() { PremisesId = premises[8].Id,  InspectionDate = now.AddDays(-42), Score = 85, Outcome = Outcome.Pass, Notes = "Clean and organised." },
                new() { PremisesId = premises[9].Id,  InspectionDate = now.AddDays(-50), Score = 33, Outcome = Outcome.Fail, Notes = "Multiple violations." },
                new() { PremisesId = premises[10].Id, InspectionDate = now.AddDays(-55), Score = 70, Outcome = Outcome.Pass, Notes = "Satisfactory." },
                new() { PremisesId = premises[11].Id, InspectionDate = now.AddDays(-60), Score = 65, Outcome = Outcome.Pass, Notes = "Minor issues noted." },
                // Older
                new() { PremisesId = premises[0].Id,  InspectionDate = now.AddDays(-90),  Score = 50, Outcome = Outcome.Fail, Notes = "Repeat hygiene issues." },
                new() { PremisesId = premises[1].Id,  InspectionDate = now.AddDays(-95),  Score = 82, Outcome = Outcome.Pass, Notes = "Consistent standards." },
                new() { PremisesId = premises[2].Id,  InspectionDate = now.AddDays(-100), Score = 48, Outcome = Outcome.Fail, Notes = "Cold chain failure." },
                new() { PremisesId = premises[3].Id,  InspectionDate = now.AddDays(-110), Score = 76, Outcome = Outcome.Pass, Notes = "Good." },
                new() { PremisesId = premises[4].Id,  InspectionDate = now.AddDays(-120), Score = 90, Outcome = Outcome.Pass, Notes = "Top marks." },
                new() { PremisesId = premises[5].Id,  InspectionDate = now.AddDays(-130), Score = 55, Outcome = Outcome.Fail, Notes = "Staff hygiene poor." },
                new() { PremisesId = premises[6].Id,  InspectionDate = now.AddDays(-140), Score = 44, Outcome = Outcome.Fail, Notes = "Pest control needed." },
                new() { PremisesId = premises[7].Id,  InspectionDate = now.AddDays(-150), Score = 80, Outcome = Outcome.Pass, Notes = "Well run." },
                new() { PremisesId = premises[8].Id,  InspectionDate = now.AddDays(-160), Score = 88, Outcome = Outcome.Pass, Notes = "Very clean." },
                new() { PremisesId = premises[9].Id,  InspectionDate = now.AddDays(-170), Score = 30, Outcome = Outcome.Fail, Notes = "Serious violations." },
                new() { PremisesId = premises[10].Id, InspectionDate = now.AddDays(-180), Score = 73, Outcome = Outcome.Pass, Notes = "Adequate." },
                new() { PremisesId = premises[11].Id, InspectionDate = now.AddDays(-190), Score = 68, Outcome = Outcome.Pass, Notes = "Acceptable." },
                new() { PremisesId = premises[0].Id,  InspectionDate = now.AddDays(-200), Score = 41, Outcome = Outcome.Fail, Notes = "Ongoing issues." },
            };

            db.Inspections.AddRange(inspections);
            await db.SaveChangesAsync();

            // ── Follow-Ups ─────────────────────────────────────
            var followUps = new List<FollowUp>
            {
                // Overdue + Open
                new() { InspectionId = inspections[0].Id,  DueDate = now.AddDays(-10), Status = FollowUpStatus.Open },
                new() { InspectionId = inspections[2].Id,  DueDate = now.AddDays(-5),  Status = FollowUpStatus.Open },
                new() { InspectionId = inspections[5].Id,  DueDate = now.AddDays(-3),  Status = FollowUpStatus.Open },
                new() { InspectionId = inspections[9].Id,  DueDate = now.AddDays(-20), Status = FollowUpStatus.Open },
                // Future + Open
                new() { InspectionId = inspections[6].Id,  DueDate = now.AddDays(7),   Status = FollowUpStatus.Open },
                new() { InspectionId = inspections[12].Id, DueDate = now.AddDays(14),  Status = FollowUpStatus.Open },
                // Closed
                new() { InspectionId = inspections[14].Id, DueDate = now.AddDays(-30), Status = FollowUpStatus.Closed, ClosedDate = now.AddDays(-25) },
                new() { InspectionId = inspections[17].Id, DueDate = now.AddDays(-60), Status = FollowUpStatus.Closed, ClosedDate = now.AddDays(-55) },
                new() { InspectionId = inspections[21].Id, DueDate = now.AddDays(-80), Status = FollowUpStatus.Closed, ClosedDate = now.AddDays(-70) },
                new() { InspectionId = inspections[24].Id, DueDate = now.AddDays(-90), Status = FollowUpStatus.Closed, ClosedDate = now.AddDays(-85) },
            };

            db.FollowUps.AddRange(followUps);
            await db.SaveChangesAsync();
        }

        // ── Helper ─────────────────────────────────────────────
        private static async Task CreateUser(UserManager<IdentityUser> userManager,
            string email, string password, string role)
        {
            if (await userManager.FindByEmailAsync(email) == null)
            {
                var user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
                await userManager.CreateAsync(user, password);
                await userManager.AddToRoleAsync(user, role);
            }
        }
    }
}
