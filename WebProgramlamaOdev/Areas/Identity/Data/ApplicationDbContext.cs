using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebProgramlamaOdev.Models;

namespace WebProgramlamaOdev.Areas.Identity.Data;

public class ApplicationDbContext : IdentityDbContext<IdentityUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // DbSet'ler - Her model için tablo
    public DbSet<Gym> Gyms { get; set; }
    public DbSet<Service> Services { get; set; }
    public DbSet<Trainer> Trainers { get; set; }
    public DbSet<TrainerAvailability> TrainerAvailabilities { get; set; }
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<AIRequest> AIRequests { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Gym - Service İlişkisi (1-to-many)
        builder.Entity<Service>()
            .HasOne(s => s.Gym)
            .WithMany(g => g.Services)
            .HasForeignKey(s => s.GymId)
            .OnDelete(DeleteBehavior.Restrict);

        // Gym - Trainer İlişkisi (1-to-many)
        builder.Entity<Trainer>()
            .HasOne(t => t.Gym)
            .WithMany(g => g.Trainers)
            .HasForeignKey(t => t.GymId)
            .OnDelete(DeleteBehavior.Restrict);

        // Trainer - TrainerAvailability İlişkisi (1-to-many)
        builder.Entity<TrainerAvailability>()
            .HasOne(ta => ta.Trainer)
            .WithMany(t => t.Availabilities)
            .HasForeignKey(ta => ta.TrainerId)
            .OnDelete(DeleteBehavior.Cascade);

        // Trainer - Service İlişkisi (Many-to-Many)
        builder.Entity<Trainer>()
            .HasMany(t => t.Services)
            .WithMany(s => s.Trainers)
            .UsingEntity(j => j.ToTable("TrainerServices"));

        // Appointment İlişkileri
        builder.Entity<Appointment>()
            .HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Appointment>()
            .HasOne(a => a.Trainer)
            .WithMany(t => t.Appointments)
            .HasForeignKey(a => a.TrainerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Appointment>()
            .HasOne(a => a.Service)
            .WithMany()
            .HasForeignKey(a => a.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        // AIRequest İlişkisi
        builder.Entity<AIRequest>()
            .HasOne(ai => ai.User)
            .WithMany()
            .HasForeignKey(ai => ai.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Decimal precision ayarları
        builder.Entity<Service>()
            .Property(s => s.Price)
            .HasPrecision(18, 2);

        builder.Entity<Appointment>()
            .Property(a => a.TotalPrice)
            .HasPrecision(18, 2);

        builder.Entity<AIRequest>()
            .Property(ai => ai.Weight)
            .HasPrecision(18, 2);
    }
}