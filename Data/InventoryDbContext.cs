using Microsoft.EntityFrameworkCore;
using SmartInventory.Models;

namespace SmartInventory.Data
{
    public class InventoryDbContext : DbContext
    {
        public InventoryDbContext(DbContextOptions<InventoryDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products => Set<Product>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Supplier> Suppliers => Set<Supplier>();
        public DbSet<StockTransaction> StockTransactions => Set<StockTransaction>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Category configuration
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Name).IsRequired().HasMaxLength(60);
                entity.HasIndex(c => c.Name).IsUnique();
            });

            // Supplier configuration
            modelBuilder.Entity<Supplier>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.Property(s => s.Name).IsRequired().HasMaxLength(100);
                entity.Property(s => s.Email).IsRequired().HasMaxLength(120);
                entity.Property(s => s.Phone).IsRequired().HasMaxLength(25);
            });

            // Product configuration
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.SKU).IsRequired().HasMaxLength(30);
                entity.HasIndex(p => p.SKU).IsUnique();

                entity.Property(p => p.Name).IsRequired().HasMaxLength(120);
                entity.Property(p => p.PurchasePrice).HasPrecision(12, 2);
                entity.Property(p => p.SellingPrice).HasPrecision(12, 2);

                // Relationships
                entity.HasOne(p => p.Category)
                      .WithMany(c => c.Products)
                      .HasForeignKey(p => p.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.Supplier)
                      .WithMany(s => s.Products)
                      .HasForeignKey(p => p.SupplierId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // StockTransaction configuration
            modelBuilder.Entity<StockTransaction>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.Property(t => t.Reason).IsRequired().HasMaxLength(250);

                entity.HasOne(t => t.Product)
                      .WithMany(p => p.StockTransactions)
                      .HasForeignKey(t => t.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
