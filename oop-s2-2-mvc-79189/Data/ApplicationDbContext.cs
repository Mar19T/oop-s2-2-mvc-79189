using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Inspections.Domain;

namespace oop_s2_2_mvc_79189.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        { }
           public DbSet<Premises> Premises { get; set; }
            public DbSet<Inspection> Inspections { get; set; }
            public DbSet<FollowUp> FollowUps { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Premises -> Inspections
            modelBuilder.Entity<Inspection>()
                .HasOne(i => i.Premises)
                .WithMany(p => p.Inspections)
                .HasForeignKey(i => i.PremisesId)
                .OnDelete(DeleteBehavior.Cascade);

            // Inspection -> FollowUps
            modelBuilder.Entity<FollowUp>()
                .HasOne(f => f.Inspection)
                .WithMany(i => i.FollowUps)
                .HasForeignKey(f => f.InspectionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}