using Microsoft.EntityFrameworkCore;
using Pos.Sales.Api.Domain;

namespace Pos.Sales.Api.Data;

public class SalesDbContext : DbContext
{
    public SalesDbContext(DbContextOptions<SalesDbContext> options) : base(options)
    {
    }

    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleItem> SaleItems => Set<SaleItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Sale>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Number).ValueGeneratedOnAdd();
            entity.HasIndex(s => s.Number).IsUnique();
            entity.Property(s => s.CashierName).HasMaxLength(100);
            entity.Property(s => s.Subtotal).HasPrecision(18, 2);
            entity.Property(s => s.TaxTotal).HasPrecision(18, 2);
            entity.Property(s => s.GrandTotal).HasPrecision(18, 2);
            entity.Property(s => s.AmountTendered).HasPrecision(18, 2);
            entity.Property(s => s.ChangeDue).HasPrecision(18, 2);
            entity.Property(s => s.PaymentMethod).HasConversion<string>().HasMaxLength(20);

            entity.HasMany(s => s.Items)
                  .WithOne(i => i.Sale!)
                  .HasForeignKey(i => i.SaleId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SaleItem>(entity =>
        {
            entity.HasKey(i => i.Id);
            entity.Property(i => i.Sku).HasMaxLength(50);
            entity.Property(i => i.Name).HasMaxLength(200);
            entity.Property(i => i.UnitPrice).HasPrecision(18, 2);
            entity.Property(i => i.TaxRate).HasPrecision(5, 4);
            entity.Property(i => i.LineSubtotal).HasPrecision(18, 2);
            entity.Property(i => i.LineTax).HasPrecision(18, 2);
            entity.Property(i => i.LineTotal).HasPrecision(18, 2);
        });
    }
}
