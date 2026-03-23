using Microsoft.EntityFrameworkCore;
using SantosSystemRise.Models;

namespace SantosSystemRise.Data;

public class SystemRiseContext : DbContext
{
    public SystemRiseContext(DbContextOptions<SystemRiseContext> options)
        : base(options)
    {
    }

    public DbSet<Device> Devices { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Device>()
            .HasKey(d => d.MacAddress);

        base.OnModelCreating(modelBuilder);
    }
}