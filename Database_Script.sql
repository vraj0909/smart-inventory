-- ============================================================
-- Smart Inventory & Stock Management System
-- MySQL Database Creation Script
-- Default Host: localhost:3306 | Database: smart_inventory_db
-- ============================================================

CREATE DATABASE IF NOT EXISTS `smart_inventory_db` 
CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

USE `smart_inventory_db`;

-- 1. Categories Table
CREATE TABLE IF NOT EXISTS `Categories` (
    `Id` INT NOT NULL AUTO_INCREMENT,
    `Name` VARCHAR(60) NOT NULL,
    `Description` VARCHAR(250) NULL,
    `Icon` VARCHAR(50) NOT NULL DEFAULT 'bi-tag',
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`Id`),
    UNIQUE INDEX `IX_Categories_Name` (`Name` ASC)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 2. Suppliers Table
CREATE TABLE IF NOT EXISTS `Suppliers` (
    `Id` INT NOT NULL AUTO_INCREMENT,
    `Name` VARCHAR(100) NOT NULL,
    `ContactPerson` VARCHAR(100) NOT NULL,
    `Email` VARCHAR(120) NOT NULL,
    `Phone` VARCHAR(25) NOT NULL,
    `Address` VARCHAR(200) NULL,
    `City` VARCHAR(80) NULL,
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 3. Products Table
CREATE TABLE IF NOT EXISTS `Products` (
    `Id` INT NOT NULL AUTO_INCREMENT,
    `SKU` VARCHAR(30) NOT NULL,
    `Name` VARCHAR(120) NOT NULL,
    `Description` VARCHAR(500) NULL,
    `CategoryId` INT NOT NULL,
    `SupplierId` INT NOT NULL,
    `PurchasePrice` DECIMAL(12, 2) NOT NULL,
    `SellingPrice` DECIMAL(12, 2) NOT NULL,
    `QuantityInStock` INT NOT NULL DEFAULT 0,
    `ReorderLevel` INT NOT NULL DEFAULT 10,
    `Unit` VARCHAR(20) NOT NULL DEFAULT 'pcs',
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` DATETIME NULL,
    PRIMARY KEY (`Id`),
    UNIQUE INDEX `IX_Products_SKU` (`SKU` ASC),
    INDEX `IX_Products_CategoryId` (`CategoryId` ASC),
    INDEX `IX_Products_SupplierId` (`SupplierId` ASC),
    CONSTRAINT `FK_Products_Categories` FOREIGN KEY (`CategoryId`) REFERENCES `Categories` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_Products_Suppliers` FOREIGN KEY (`SupplierId`) REFERENCES `Suppliers` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 4. StockTransactions (Audit & Movement Log) Table
CREATE TABLE IF NOT EXISTS `StockTransactions` (
    `Id` INT NOT NULL AUTO_INCREMENT,
    `ProductId` INT NOT NULL,
    `TransactionType` INT NOT NULL COMMENT '1=StockIn, 2=StockOut, 3=Adjustment, 4=Return',
    `Quantity` INT NOT NULL,
    `PreviousStock` INT NOT NULL DEFAULT 0,
    `NewStock` INT NOT NULL DEFAULT 0,
    `Reason` VARCHAR(250) NOT NULL,
    `PerformedBy` VARCHAR(60) NOT NULL DEFAULT 'Store Admin',
    `Timestamp` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`Id`),
    INDEX `IX_StockTransactions_ProductId` (`ProductId` ASC),
    CONSTRAINT `FK_StockTransactions_Products` FOREIGN KEY (`ProductId`) REFERENCES `Products` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ============================================================
-- Initial Demo Retail Seed Data
-- ============================================================

INSERT IGNORE INTO `Categories` (`Id`, `Name`, `Description`, `Icon`, `CreatedAt`) VALUES
(1, 'Electronics & Gadgets', 'Peripherals, cables, chargers, and accessories', 'bi-laptop', NOW()),
(2, 'Office & Stationery', 'Writing materials, papers, files, and desk items', 'bi-pencil-square', NOW()),
(3, 'Hardware & Tools', 'Fixing tools, batteries, mounting hardware', 'bi-tools', NOW()),
(4, 'Snacks & Refreshments', 'Packaged retail beverages and counter snacks', 'bi-cup-hot', NOW()),
(5, 'Packaging Materials', 'Boxes, tapes, bubble wraps, and courier bags', 'bi-box-seam', NOW());

INSERT IGNORE INTO `Suppliers` (`Id`, `Name`, `ContactPerson`, `Email`, `Phone`, `Address`, `City`, `CreatedAt`) VALUES
(1, 'Apex Tech Distribution', 'Marcus Vance', 'orders@apextech.com', '+1 (555) 234-8901', '104 Silicon Parkway, Suite 300', 'San Jose', NOW()),
(2, 'Evergreen Office Supply Co.', 'Elena Rostova', 'supply@evergreenoffice.com', '+1 (555) 890-1234', '742 Commercial Way', 'Chicago', NOW()),
(3, 'Vanguard Tools & Fasteners', 'David Miller', 'sales@vanguardtools.net', '+1 (555) 456-7890', '88 Industrial Blvd', 'Dallas', NOW()),
(4, 'Metro Wholesale Refreshments', 'Sarah Jenkins', 'orders@metrowholesale.com', '+1 (555) 321-6549', '500 Market Square', 'Atlanta', NOW());

INSERT IGNORE INTO `Products` (`Id`, `SKU`, `Name`, `Description`, `CategoryId`, `SupplierId`, `PurchasePrice`, `SellingPrice`, `QuantityInStock`, `ReorderLevel`, `Unit`, `IsActive`, `CreatedAt`) VALUES
(1, 'ELEC-1001', 'Ergonomic Wireless Mouse 2.4G', 'Silent click, dual DPI settings with USB nano receiver.', 1, 1, 12.50, 24.99, 4, 10, 'pcs', 1, NOW()),
(2, 'ELEC-1002', 'Braided USB-C Fast Charging Cable 2M', 'Durable nylon braided 60W power delivery cable.', 1, 1, 3.20, 9.99, 3, 15, 'pcs', 1, NOW()),
(3, 'ELEC-1003', 'Mechanical Keyboard Blue Switch RGB', 'Tactile mechanical gaming and typing keyboard with wrist rest.', 1, 1, 28.00, 59.99, 0, 8, 'pcs', 1, NOW()),
(4, 'ELEC-1004', 'USB 3.0 4-Port Aluminum Hub', 'High speed 5Gbps hub with LED indicator.', 1, 1, 8.50, 19.99, 22, 10, 'pcs', 1, NOW()),
(5, 'STAT-2001', 'Premium Gel Pens 0.5mm (Pack of 12)', 'Quick-dry smudge proof black ink pens.', 2, 2, 4.10, 8.50, 5, 12, 'box', 1, NOW()),
(6, 'STAT-2002', 'A4 Multipurpose Copy Paper 500 Sheets', 'Bright white 80 GSM paper for laser & inkjet printers.', 2, 2, 3.80, 7.00, 45, 20, 'ream', 1, NOW()),
(7, 'STAT-2003', 'Self-Adhesive Neon Sticky Notes 3x3', 'Pack of 6 vibrant pads with ultra-stick adhesive.', 2, 2, 2.00, 4.99, 0, 15, 'pack', 1, NOW()),
(8, 'HARD-3001', 'Precision Screwdriver Set 32-in-1', 'Magnetic bits set for mobile phones, laptops, and watches.', 3, 3, 7.50, 18.00, 14, 8, 'set', 1, NOW()),
(9, 'HARD-3002', 'Heavy Duty Double-Sided Foam Tape', 'Strong mounting tape 5m x 20mm weather-resistant.', 3, 3, 2.10, 5.50, 2, 10, 'roll', 1, NOW()),
(10, 'SNK-4001', 'Organic Arabica Ground Coffee 250g', 'Medium roast single-origin rich aroma ground coffee.', 4, 4, 4.50, 9.50, 18, 10, 'bag', 1, NOW()),
(11, 'SNK-4002', 'Artisan Dark Chocolate Almond Bar', '70% cocoa with roasted California almonds.', 4, 4, 1.30, 3.25, 6, 15, 'bar', 1, NOW()),
(12, 'PKG-5001', 'Corrugated Shipping Boxes (12x9x4 inch)', 'Bundle of 25 strong cardboard mailing boxes.', 5, 2, 11.00, 22.50, 30, 10, 'bundle', 1, NOW());

INSERT IGNORE INTO `StockTransactions` (`Id`, `ProductId`, `TransactionType`, `Quantity`, `PreviousStock`, `NewStock`, `Reason`, `PerformedBy`, `Timestamp`) VALUES
(1, 1, 1, 20, 0, 20, 'Initial inventory intake from PO #APX-981', 'Inventory Admin', NOW()),
(2, 1, 2, 16, 20, 4, 'Customer retail sales (POS batch #401)', 'Store Cashier', NOW()),
(3, 3, 1, 10, 0, 10, 'Supplier replenishment batch', 'Inventory Admin', NOW()),
(4, 3, 2, 10, 10, 0, 'Bulk clearance sale - stock depleted', 'Sales Lead', NOW()),
(5, 6, 1, 50, 0, 50, 'Bulk warehouse delivery PO #EVR-202', 'Inventory Admin', NOW()),
(6, 6, 2, 5, 50, 45, 'Office supply dispatch', 'Operations Staff', NOW());
