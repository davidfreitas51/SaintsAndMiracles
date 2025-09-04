using Core.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class DataContext(DbContextOptions<DataContext> options) : IdentityDbContext<AppUser>(options)
{
    public DbSet<Saint> Saints { get; set; }
    public DbSet<Miracle> Miracles { get; set; }
    public DbSet<Prayer> Prayers { get; set; }
    public DbSet<ReligiousOrder> ReligiousOrders { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<AccountToken> AccountTokens { get; set; } = null!;
    public DbSet<RecentActivity> RecentActivities { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Saint>()
            .HasMany(s => s.Tags)
            .WithMany(t => t.Saints)
            .UsingEntity(j => j.ToTable("SaintTags"));

        modelBuilder.Entity<Miracle>()
            .HasMany(m => m.Tags)
            .WithMany(t => t.Miracles)
            .UsingEntity(j => j.ToTable("MiracleTags"));

        modelBuilder.Entity<Prayer>()
            .HasMany(p => p.Tags)
            .WithMany(t => t.Prayers)
            .UsingEntity(j => j.ToTable("PrayerTags"));
    }
}
