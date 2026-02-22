using DataAccess.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DataAccess;

public class MyDbContext : IdentityDbContext<AppUser, AppRole, Guid>
{
    public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
    {
    }
    
    public DbSet<Event> Events => Set<Event>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Title)
                .HasMaxLength(200);

            //Used for range queries (show all events within the range visible in the client)
            entity.HasIndex(e => new
            {
                e.OwnerId,
                e.StartUtc,
                e.EndUtc,
            });

            entity.HasOne<AppUser>()
                .WithMany()
                .HasForeignKey(e => e.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.Property(e => e.TimeZoneId)
                .HasMaxLength(100);
            
            entity.Property(e => e.StartDate)
                .HasColumnType("date");
            
            entity.Property(e => e.EndDate)
                .HasColumnType("date");
        });
    }
    
    
}