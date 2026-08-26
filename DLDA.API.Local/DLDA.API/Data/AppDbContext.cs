using DLDA.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DLDA.API.Data
{
    /// <summary>
    /// Serves as the primary Entity Framework database context for the application,
    /// defining database sets and configuring relational mappings and cascading deletion behaviors.
    /// </summary>
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Assessment> Assessments { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<AssessmentItem> AssessmentItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure cascade deletion to ensure related assessment items are purged when an assessment container is removed
            modelBuilder.Entity<Assessment>()
                .HasMany(a => a.AssessmentItems)
                .WithOne(ai => ai.Assessment)
                .HasForeignKey(ai => ai.AssessmentID)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure cascade deletion to clean up associated user assessments when a user account is deleted
            modelBuilder.Entity<User>()
                .HasMany(u => u.Assessments)
                .WithOne(a => a.User)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}