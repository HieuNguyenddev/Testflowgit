using FlowGit.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace FlowGit.API.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(150)
                .UseCollation("Vietnamese_100_CI_AI");
            entity.Property(e => e.Price).HasPrecision(18, 2);
            entity.Property<string>("NameSearch")
                .IsRequired()
                .HasMaxLength(150);

            entity.HasData(
                Seed(1, "Bàn phím cơ", 1_290_000m, 15),
                Seed(2, "Chuột không dây", 450_000m, 30),
                Seed(3, "Màn hình 27 inch", 5_490_000m, 8),
                Seed(4, "Tai nghe Bluetooth", 890_000m, 20),
                Seed(5, "Laptop 14 inch", 18_900_000m, 5),
                Seed(6, "Ổ cứng SSD 1TB", 1_750_000m, 12),
                Seed(7, "Bàn phím số", 320_000m, 18),
                Seed(8, "Chuột gaming", 690_000m, 22),
                Seed(9, "Webcam Full HD", 550_000m, 11),
                Seed(10, "Loa vi tính", 410_000m, 16),
                Seed(11, "Bàn di chuột", 150_000m, 40),
                Seed(12, "Cáp HDMI 2m", 120_000m, 50));
        });
    }

    private static object Seed(int id, string name, decimal price, int quantity) => new
    {
        Id = id,
        Name = name,
        Price = price,
        Quantity = quantity,
        NameSearch = TextSearch.Normalize(name)
    };
}
