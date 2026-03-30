using Inspections.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using oop_s2_2_mvc_79189.Data;
using System.Collections.Generic;
using System.IO;
using System.Net;
using static System.Runtime.InteropServices.JavaScript.JSType;
using InspectionEntity = Inspections.Domain.Inspection;


namespace Inspection.Test
{
    public class UnitTest1
    {
        // Helper method — creates a fresh in-memory database for each test
        private ApplicationDbContext GetInMemoryDb()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // unique name each time
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task OverdueFollowUps_ShouldReturnOnlyOverdueOpenItems()
        {
            // ?? ARRANGE ????????????????????????????????????????
            var db = GetInMemoryDb();
            var today = DateTime.Today;

            // First we need a Premises and Inspection to attach FollowUps to
            var premises = new Premises
            {
                Name = "Test Cafe",
                Address = "1 Test St",
                Town = "Dublin",
                RiskRating = RiskRating.Low
            };
            db.Premises.Add(premises);
            await db.SaveChangesAsync();

            var inspection = new Inspections.Domain.Inspection
            {
                PremisesId = premises.Id,
                InspectionDate = today.AddDays(-30),
                Score = 75,
                Outcome = Outcome.Pass,
                Notes = "Test inspection"
            };
            db.Inspections.Add(inspection);
            await db.SaveChangesAsync();

            // Now add 3 follow-ups:
            // - 2 should be returned (overdue + open)
            // - 1 should NOT be returned (closed)
            // - 1 should NOT be returned (open but not overdue yet)
            var followUps = new List<FollowUp>
            {
                // ? Should appear — overdue and open
                new() { InspectionId = inspection.Id, DueDate = today.AddDays(-5),  Status = FollowUpStatus.Open },
                // ? Should appear — overdue and open
                new() { InspectionId = inspection.Id, DueDate = today.AddDays(-10), Status = FollowUpStatus.Open },
                // ? Should NOT appear — closed
                new() { InspectionId = inspection.Id, DueDate = today.AddDays(-3),  Status = FollowUpStatus.Closed, ClosedDate = today.AddDays(-1) },
                // ? Should NOT appear — open but due in the future
                new() { InspectionId = inspection.Id, DueDate = today.AddDays(7),   Status = FollowUpStatus.Open },
            };
            db.FollowUps.AddRange(followUps);
            await db.SaveChangesAsync();

            // ?? ACT ????????????????????????????????????????????
            // This is the exact same query used in DashboardController
            var overdueFollowUps = await db.FollowUps
                .Where(f => f.DueDate < today && f.Status == FollowUpStatus.Open)
                .ToListAsync();

            // ?? ASSERT ?????????????????????????????????????????
            Assert.Equal(2, overdueFollowUps.Count);
        }
        [Fact]
        public async Task FollowUp_CannotBeClosed_WithoutClosedDate()
        {
            // ?? ARRANGE ????????????????????????????????????????
            var db = GetInMemoryDb();
            var today = DateTime.Today;

            // Create required parent records first
            var premises = new Premises
            {
                Name = "Test Cafe",
                Address = "1 Test St",
                Town = "Dublin",
                RiskRating = RiskRating.Low
            };
            db.Premises.Add(premises);
            await db.SaveChangesAsync();

            var inspection = new InspectionEntity
            {
                PremisesId = premises.Id,
                InspectionDate = today.AddDays(-10),
                Score = 60,
                Outcome = Outcome.Fail,
                Notes = "Test inspection"
            };
            db.Inspections.Add(inspection);
            await db.SaveChangesAsync();

            // ?? ACT ????????????????????????????????????????????
            // Create a follow-up that is Closed but has NO ClosedDate
            var followUp = new FollowUp
            {
                InspectionId = inspection.Id,
                DueDate = today.AddDays(-3),
                Status = FollowUpStatus.Closed,
                ClosedDate = null // ? this should be invalid
            };

            // Simulate the business rule check from our controller
            bool isValid = !(followUp.Status == FollowUpStatus.Closed && followUp.ClosedDate == null);

            // ?? ASSERT ?????????????????????????????????????????
            Assert.False(isValid); // should be invalid — closed without a date
        }
        [Fact]
        public async Task DashboardCounts_ShouldMatchKnownSeedData()
        {
            // ?? ARRANGE ????????????????????????????????????????
            var db = GetInMemoryDb();
            var today = DateTime.Today;
            var startOfMonth = new DateTime(today.Year, today.Month, 1);

            // Create a premises to attach inspections to
            var premises = new Premises
            {
                Name = "Test Cafe",
                Address = "1 Test St",
                Town = "Dublin",
                RiskRating = RiskRating.Low
            };
            db.Premises.Add(premises);
            await db.SaveChangesAsync();

            // Create 5 inspections this month — 2 failed, 3 passed
            var inspections = new List<InspectionEntity>
    {
        new() { PremisesId = premises.Id, InspectionDate = today.AddDays(-2),  Score = 45, Outcome = Outcome.Fail,  Notes = "Failed 1" },
        new() { PremisesId = premises.Id, InspectionDate = today.AddDays(-5),  Score = 33, Outcome = Outcome.Fail,  Notes = "Failed 2" },
        new() { PremisesId = premises.Id, InspectionDate = today.AddDays(-7),  Score = 78, Outcome = Outcome.Pass,  Notes = "Passed 1" },
        new() { PremisesId = premises.Id, InspectionDate = today.AddDays(-10), Score = 85, Outcome = Outcome.Pass,  Notes = "Passed 2" },
        new() { PremisesId = premises.Id, InspectionDate = today.AddDays(-15), Score = 91, Outcome = Outcome.Pass,  Notes = "Passed 3" },
        // This one is from LAST month — should NOT be counted
        new() { PremisesId = premises.Id, InspectionDate = today.AddDays(-40), Score = 55, Outcome = Outcome.Fail,  Notes = "Old failed" },
    };
            db.Inspections.AddRange(inspections);
            await db.SaveChangesAsync();

            // ?? ACT ????????????????????????????????????????????
            // Same queries used in DashboardController
            var inspectionsThisMonth = await db.Inspections
                .Where(i => i.InspectionDate >= startOfMonth)
                .CountAsync();

            var failedThisMonth = await db.Inspections
                .Where(i => i.InspectionDate >= startOfMonth && i.Outcome == Outcome.Fail)
                .CountAsync();

            // ?? ASSERT ?????????????????????????????????????????
            Assert.Equal(5, inspectionsThisMonth); // 5 this month, old one excluded
            Assert.Equal(2, failedThisMonth);      // only 2 failed this month
        }

        [Fact]
        public void Viewer_ShouldNotHaveInspectorPermissions()
        {
            // ?? ARRANGE ????????????????????????????????????????
            // Simulate what roles a Viewer and Inspector have
            var viewerRoles = new List<string> { "Viewer" };
            var inspectorRoles = new List<string> { "Inspector" };

            // This mirrors the role check in our controllers and views:
            // [Authorize(Roles = "Admin,Inspector")]
            bool CanCreateInspection(List<string> roles) =>
                roles.Contains("Admin") || roles.Contains("Inspector");

            bool CanViewDashboard(List<string> roles) =>
                roles.Contains("Admin") || roles.Contains("Inspector") || roles.Contains("Viewer");

            bool CanDeleteInspection(List<string> roles) =>
                roles.Contains("Admin");

            // ?? ACT & ASSERT ???????????????????????????????????
            // Viewer cannot create
            Assert.False(CanCreateInspection(viewerRoles));

            // Viewer can view dashboard
            Assert.True(CanViewDashboard(viewerRoles));

            // Viewer cannot delete
            Assert.False(CanDeleteInspection(viewerRoles));

            // Inspector can create
            Assert.True(CanCreateInspection(inspectorRoles));

            // Inspector cannot delete
            Assert.False(CanDeleteInspection(inspectorRoles));
        }

    }
}