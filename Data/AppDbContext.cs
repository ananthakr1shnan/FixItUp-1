using FixItUp.Models;
using Microsoft.EntityFrameworkCore;

namespace FixItUp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // ======================
        // DB SETS
        // ======================
        public DbSet<User> Users { get; set; }
        public DbSet<TaskEntity> Tasks { get; set; }
        public DbSet<Bid> Bids { get; set; }
        public DbSet<Dispute> Disputes { get; set; }
        public DbSet<TaskChecklistItem> TaskChecklistItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ======================
            // USER CONFIGURATION
            // ======================
            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(u => u.Role)
                      .IsRequired();

                entity.Property(u => u.TrustScore)
                      .HasDefaultValue(50);

                entity.Property(u => u.IsAcceptingJobs)
                      .HasDefaultValue(true);

                entity.Property(u => u.AvailableBalance)
                      .HasPrecision(18, 2)
                      .HasDefaultValue(0);

                entity.Property(u => u.PendingClearance)
                      .HasPrecision(18, 2)
                      .HasDefaultValue(0);
            });

            // ======================
            // TASK CONFIGURATION
            // ======================
            modelBuilder.Entity<TaskEntity>(entity =>
            {
                entity.Property(t => t.Status)
                      .IsRequired();

                entity.Property(t => t.IsUrgent)
                      .HasDefaultValue(false);

                entity.Property(t => t.MinBudget)
                      .HasPrecision(18, 2);

                entity.Property(t => t.MaxBudget)
                      .HasPrecision(18, 2);

                entity.Property(t => t.CreatedAt)
                      .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne<User>()
                      .WithMany()
                      .HasForeignKey(t => t.CustomerId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<User>()
                      .WithMany()
                      .HasForeignKey(t => t.AssignedWorkerId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ======================
            // BID CONFIGURATION
            // ======================
            modelBuilder.Entity<Bid>(entity =>
            {
                entity.Property(b => b.Amount)
                      .HasPrecision(18, 2)
                      .IsRequired();

                entity.HasOne<TaskEntity>()
                      .WithMany()
                      .HasForeignKey(b => b.TaskId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne<User>()
                      .WithMany()
                      .HasForeignKey(b => b.WorkerId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ======================
            // DISPUTE CONFIGURATION
            // ======================
            modelBuilder.Entity<Dispute>(entity =>
            {
                entity.Property(d => d.Status)
                      .HasDefaultValue("Open");
            });

            // ======================
            // TASK CHECKLIST CONFIGURATION
            // ======================
            modelBuilder.Entity<TaskChecklistItem>(entity =>
            {
                entity.Property(c => c.TaskItem)
                      .IsRequired();

                entity.Property(c => c.IsDone)
                      .HasDefaultValue(false);

                entity.HasOne<TaskEntity>()
                      .WithMany()
                      .HasForeignKey(c => c.TaskId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
