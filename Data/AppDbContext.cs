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

        public DbSet<User> Users { get; set; }
        public DbSet<TaskEntity> Tasks { get; set; }
        public DbSet<Bid> Bids { get; set; }
        public DbSet<Dispute> Disputes { get; set; }
        public DbSet<TaskChecklistItem> TaskChecklistItems { get; set; }
        public DbSet<ServiceCategory> ServiceCategories { get; set; }
        public DbSet<WorkerSkill> WorkerSkills { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ======================
            // SKILLS CONFIGURATION
            // ======================
            modelBuilder.Entity<WorkerSkill>()
                .HasKey(ws => new { ws.UserId, ws.CategoryId });

            modelBuilder.Entity<WorkerSkill>()
                .HasOne(ws => ws.User)
                .WithMany(u => u.SpecializedSkills)
                .HasForeignKey(ws => ws.UserId);

            modelBuilder.Entity<WorkerSkill>()
                .HasOne(ws => ws.Category)
                .WithMany()
                .HasForeignKey(ws => ws.CategoryId);

            // Seed Categories
            modelBuilder.Entity<ServiceCategory>().HasData(
                new ServiceCategory { Id = 1, Name = "Plumbing" },
                new ServiceCategory { Id = 2, Name = "Electrical" },
                new ServiceCategory { Id = 3, Name = "Carpentry" },
                new ServiceCategory { Id = 4, Name = "Painting" },
                new ServiceCategory { Id = 5, Name = "Assembly" },
                new ServiceCategory { Id = 6, Name = "Cleaning" },
                new ServiceCategory { Id = 7, Name = "Maintenance" },
                new ServiceCategory { Id = 8, Name = "Moving" }
            );

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

            // Seed Admin User
            modelBuilder.Entity<User>().HasData(new User
            {
                Id = 999, // Static ID for Admin
                FullName = "System Administrator",
                Email = "admin@fixitup.com",
                PasswordHash = "admin123", // Plain text for demo; in production use hashing
                Role = "Admin",
                IsAcceptingJobs = false,
                TrustScore = 100,
                IsVerifiedPro = true,
                JobCompletionRate = 0,
                AvailableBalance = 0,
                PendingClearance = 0
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
                      .HasDefaultValueSql("GETUTCDATE()");

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

                entity.Property(b => b.CreatedAt)
                      .HasDefaultValueSql("GETUTCDATE()");

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

            // ======================
            // PAYMENT CONFIGURATION
            // ======================
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.Property(p => p.Amount)
                      .HasPrecision(18, 2)
                      .IsRequired();

                entity.Property(p => p.Status)
                      .IsRequired()
                      .HasDefaultValue("Pending");

                entity.Property(p => p.CreatedAt)
                      .HasDefaultValueSql("GETUTCDATE()");

                // Task relationship with cascade delete
                entity.HasOne(p => p.Task)
                      .WithMany()
                      .HasForeignKey(p => p.TaskId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Customer relationship with no action to avoid cascade cycles
                entity.HasOne(p => p.Customer)
                      .WithMany()
                      .HasForeignKey(p => p.CustomerId)
                      .OnDelete(DeleteBehavior.NoAction);

                // Worker relationship with no action to avoid cascade cycles
                entity.HasOne(p => p.Worker)
                      .WithMany()
                      .HasForeignKey(p => p.WorkerId)
                      .OnDelete(DeleteBehavior.NoAction);
            });
        }
    }
}
