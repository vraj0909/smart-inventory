using System;
using System.Collections.Generic;
using System.Linq;
using SmartInventory.Models;

namespace SmartInventory.Data
{
    public static class DbInitializer
    {
        public static void Initialize(InventoryDbContext context)
        {
            context.Database.EnsureCreated();

            // Look for any products
            if (context.Products.Any())
            {
                return; // DB has already been seeded
            }

            // 1. Seed Categories
            var categories = new List<Category>
            {
                new Category { Name = "Electronics & Gadgets", Description = "Peripherals, cables, chargers, and accessories", Icon = "bi-laptop" },
                new Category { Name = "Office & Stationery", Description = "Writing materials, papers, files, and desk items", Icon = "bi-pencil-square" },
                new Category { Name = "Hardware & Tools", Description = "Fixing tools, batteries, mounting hardware", Icon = "bi-tools" },
                new Category { Name = "Snacks & Refreshments", Description = "Packaged retail beverages and counter snacks", Icon = "bi-cup-hot" },
                new Category { Name = "Packaging Materials", Description = "Boxes, tapes, bubble wraps, and courier bags", Icon = "bi-box-seam" }
            };
            context.Categories.AddRange(categories);
            context.SaveChanges();

            // 2. Seed Suppliers
            var suppliers = new List<Supplier>
            {
                new Supplier
                {
                    Name = "Apex Tech Distribution",
                    ContactPerson = "Marcus Vance",
                    Email = "orders@apextech.com",
                    Phone = "+1 (555) 234-8901",
                    Address = "104 Silicon Parkway, Suite 300",
                    City = "San Jose"
                },
                new Supplier
                {
                    Name = "Evergreen Office Supply Co.",
                    ContactPerson = "Elena Rostova",
                    Email = "supply@evergreenoffice.com",
                    Phone = "+1 (555) 890-1234",
                    Address = "742 Commercial Way",
                    City = "Chicago"
                },
                new Supplier
                {
                    Name = "Vanguard Tools & Fasteners",
                    ContactPerson = "David Miller",
                    Email = "sales@vanguardtools.net",
                    Phone = "+1 (555) 456-7890",
                    Address = "88 Industrial Blvd",
                    City = "Dallas"
                },
                new Supplier
                {
                    Name = "Metro Wholesale Refreshments",
                    ContactPerson = "Sarah Jenkins",
                    Email = "orders@metrowholesale.com",
                    Phone = "+1 (555) 321-6549",
                    Address = "500 Market Square",
                    City = "Atlanta"
                }
            };
            context.Suppliers.AddRange(suppliers);
            context.SaveChanges();

            // 3. Seed Products (Mix of Normal, Low Stock, and Out of Stock)
            var catElectronics = categories[0].Id;
            var catStationery = categories[1].Id;
            var catHardware = categories[2].Id;
            var catSnacks = categories[3].Id;
            var catPackaging = categories[4].Id;

            var supTech = suppliers[0].Id;
            var supOffice = suppliers[1].Id;
            var supTools = suppliers[2].Id;
            var supSnacks = suppliers[3].Id;

            var products = new List<Product>
            {
                // Electronics
                new Product
                {
                    SKU = "ELEC-1001",
                    Name = "Ergonomic Wireless Mouse 2.4G",
                    Description = "Silent click, dual DPI settings with USB nano receiver.",
                    CategoryId = catElectronics,
                    SupplierId = supTech,
                    PurchasePrice = 12.50m,
                    SellingPrice = 24.99m,
                    QuantityInStock = 4, // LOW STOCK (Threshold 10)
                    ReorderLevel = 10,
                    Unit = "pcs",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-30)
                },
                new Product
                {
                    SKU = "ELEC-1002",
                    Name = "Braided USB-C Fast Charging Cable 2M",
                    Description = "Durable nylon braided 60W power delivery cable.",
                    CategoryId = catElectronics,
                    SupplierId = supTech,
                    PurchasePrice = 3.20m,
                    SellingPrice = 9.99m,
                    QuantityInStock = 3, // LOW STOCK (Threshold 15)
                    ReorderLevel = 15,
                    Unit = "pcs",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-28)
                },
                new Product
                {
                    SKU = "ELEC-1003",
                    Name = "Mechanical Keyboard Blue Switch RGB",
                    Description = "Tactile mechanical gaming and typing keyboard with wrist rest.",
                    CategoryId = catElectronics,
                    SupplierId = supTech,
                    PurchasePrice = 28.00m,
                    SellingPrice = 59.99m,
                    QuantityInStock = 0, // OUT OF STOCK
                    ReorderLevel = 8,
                    Unit = "pcs",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-25)
                },
                new Product
                {
                    SKU = "ELEC-1004",
                    Name = "USB 3.0 4-Port Aluminum Hub",
                    Description = "High speed 5Gbps hub with LED indicator.",
                    CategoryId = catElectronics,
                    SupplierId = supTech,
                    PurchasePrice = 8.50m,
                    SellingPrice = 19.99m,
                    QuantityInStock = 22, // Healthy stock
                    ReorderLevel = 10,
                    Unit = "pcs",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-20)
                },

                // Stationery
                new Product
                {
                    SKU = "STAT-2001",
                    Name = "Premium Gel Pens 0.5mm (Pack of 12)",
                    Description = "Quick-dry smudge proof black ink pens.",
                    CategoryId = catStationery,
                    SupplierId = supOffice,
                    PurchasePrice = 4.10m,
                    SellingPrice = 8.50m,
                    QuantityInStock = 5, // LOW STOCK (Threshold 12)
                    ReorderLevel = 12,
                    Unit = "box",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-18)
                },
                new Product
                {
                    SKU = "STAT-2002",
                    Name = "A4 Multipurpose Copy Paper 500 Sheets",
                    Description = "Bright white 80 GSM paper for laser & inkjet printers.",
                    CategoryId = catStationery,
                    SupplierId = supOffice,
                    PurchasePrice = 3.80m,
                    SellingPrice = 7.00m,
                    QuantityInStock = 45, // Healthy stock
                    ReorderLevel = 20,
                    Unit = "ream",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-15)
                },
                new Product
                {
                    SKU = "STAT-2003",
                    Name = "Self-Adhesive Neon Sticky Notes 3x3",
                    Description = "Pack of 6 vibrant pads with ultra-stick adhesive.",
                    CategoryId = catStationery,
                    SupplierId = supOffice,
                    PurchasePrice = 2.00m,
                    SellingPrice = 4.99m,
                    QuantityInStock = 0, // OUT OF STOCK
                    ReorderLevel = 15,
                    Unit = "pack",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-14)
                },

                // Hardware
                new Product
                {
                    SKU = "HARD-3001",
                    Name = "Precision Screwdriver Set 32-in-1",
                    Description = "Magnetic bits set for mobile phones, laptops, and watches.",
                    CategoryId = catHardware,
                    SupplierId = supTools,
                    PurchasePrice = 7.50m,
                    SellingPrice = 18.00m,
                    QuantityInStock = 14, // Healthy stock
                    ReorderLevel = 8,
                    Unit = "set",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-12)
                },
                new Product
                {
                    SKU = "HARD-3002",
                    Name = "Heavy Duty Double-Sided Foam Tape",
                    Description = "Strong mounting tape 5m x 20mm weather-resistant.",
                    CategoryId = catHardware,
                    SupplierId = supTools,
                    PurchasePrice = 2.10m,
                    SellingPrice = 5.50m,
                    QuantityInStock = 2, // LOW STOCK (Threshold 10)
                    ReorderLevel = 10,
                    Unit = "roll",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-10)
                },

                // Refreshments
                new Product
                {
                    SKU = "SNK-4001",
                    Name = "Organic Arabica Ground Coffee 250g",
                    Description = "Medium roast single-origin rich aroma ground coffee.",
                    CategoryId = catSnacks,
                    SupplierId = supSnacks,
                    PurchasePrice = 4.50m,
                    SellingPrice = 9.50m,
                    QuantityInStock = 18, // Healthy stock
                    ReorderLevel = 10,
                    Unit = "bag",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-8)
                },
                new Product
                {
                    SKU = "SNK-4002",
                    Name = "Artisan Dark Chocolate Almond Bar",
                    Description = "70% cocoa with roasted California almonds.",
                    CategoryId = catSnacks,
                    SupplierId = supSnacks,
                    PurchasePrice = 1.30m,
                    SellingPrice = 3.25m,
                    QuantityInStock = 6, // LOW STOCK (Threshold 15)
                    ReorderLevel = 15,
                    Unit = "bar",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-7)
                },

                // Packaging
                new Product
                {
                    SKU = "PKG-5001",
                    Name = "Corrugated Shipping Boxes (12x9x4 inch)",
                    Description = "Bundle of 25 strong cardboard mailing boxes.",
                    CategoryId = catPackaging,
                    SupplierId = supOffice,
                    PurchasePrice = 11.00m,
                    SellingPrice = 22.50m,
                    QuantityInStock = 30, // Healthy stock
                    ReorderLevel = 10,
                    Unit = "bundle",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-5)
                }
            };

            context.Products.AddRange(products);
            context.SaveChanges();

            // 4. Seed Audit Transactions
            var p1 = products[0]; // Mouse
            var p2 = products[2]; // Keyboard (out of stock)
            var p3 = products[5]; // Paper

            var transactions = new List<StockTransaction>
            {
                new StockTransaction
                {
                    ProductId = p1.Id,
                    TransactionType = TransactionType.StockIn,
                    Quantity = 20,
                    PreviousStock = 0,
                    NewStock = 20,
                    Reason = "Initial inventory intake from PO #APX-981",
                    PerformedBy = "Inventory Admin",
                    Timestamp = DateTime.UtcNow.AddDays(-20)
                },
                new StockTransaction
                {
                    ProductId = p1.Id,
                    TransactionType = TransactionType.StockOut,
                    Quantity = 16,
                    PreviousStock = 20,
                    NewStock = 4,
                    Reason = "Customer retail sales (POS batch #401)",
                    PerformedBy = "Store Cashier",
                    Timestamp = DateTime.UtcNow.AddDays(-2)
                },
                new StockTransaction
                {
                    ProductId = p2.Id,
                    TransactionType = TransactionType.StockIn,
                    Quantity = 10,
                    PreviousStock = 0,
                    NewStock = 10,
                    Reason = "Supplier replenishment batch",
                    PerformedBy = "Inventory Admin",
                    Timestamp = DateTime.UtcNow.AddDays(-15)
                },
                new StockTransaction
                {
                    ProductId = p2.Id,
                    TransactionType = TransactionType.StockOut,
                    Quantity = 10,
                    PreviousStock = 10,
                    NewStock = 0,
                    Reason = "Bulk clearance sale - stock depleted",
                    PerformedBy = "Sales Lead",
                    Timestamp = DateTime.UtcNow.AddDays(-1)
                },
                new StockTransaction
                {
                    ProductId = p3.Id,
                    TransactionType = TransactionType.StockIn,
                    Quantity = 50,
                    PreviousStock = 0,
                    NewStock = 50,
                    Reason = "Bulk warehouse delivery PO #EVR-202",
                    PerformedBy = "Inventory Admin",
                    Timestamp = DateTime.UtcNow.AddDays(-10)
                },
                new StockTransaction
                {
                    ProductId = p3.Id,
                    TransactionType = TransactionType.StockOut,
                    Quantity = 5,
                    PreviousStock = 50,
                    NewStock = 45,
                    Reason = "Office supply dispatch",
                    PerformedBy = "Operations Staff",
                    Timestamp = DateTime.UtcNow.AddHours(-12)
                }
            };

            context.StockTransactions.AddRange(transactions);
            context.SaveChanges();
        }
    }
}
