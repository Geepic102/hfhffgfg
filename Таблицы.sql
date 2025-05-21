CREATE DATABASE magazin;
use magazin;
CREATE TABLE Manufacturers (
    ManufacturerId INT PRIMARY KEY AUTO_INCREMENT,
    Name VARCHAR(100) NOT NULL
);
CREATE TABLE Suppliers (
    SupplierId INT PRIMARY KEY AUTO_INCREMENT,
    Name VARCHAR(100) NOT NULL
);
CREATE TABLE Categories (
    CategoryId INT PRIMARY KEY AUTO_INCREMENT,
    Name VARCHAR(100) NOT NULL
);
CREATE TABLE Products (
    ProductId INT PRIMARY KEY AUTO_INCREMENT,
    Article VARCHAR(50) UNIQUE NOT NULL,
    Name VARCHAR(255) NOT NULL,
    Description TEXT,
    ImageUrl VARCHAR(255),
    StockQuantity INT DEFAULT 0
);
CREATE TABLE ProductDetails (
    DetailId INT PRIMARY KEY AUTO_INCREMENT,
    ProductId INT NOT NULL,
    ManufacturerId INT,
    SupplierId INT,
    CategoryId INT,
    Unit VARCHAR(20) NOT NULL,
    Price DECIMAL(10, 2) NOT NULL,
    FOREIGN KEY (ProductId) REFERENCES Products(ProductId),
    FOREIGN KEY (ManufacturerId) REFERENCES Manufacturers(ManufacturerId),
    FOREIGN KEY (SupplierId) REFERENCES Suppliers(SupplierId),
    FOREIGN KEY (CategoryId) REFERENCES Categories(CategoryId)
);
CREATE TABLE Discounts (
    DiscountId INT PRIMARY KEY AUTO_INCREMENT,
    ProductId INT NOT NULL,
    DiscountPercentage DECIMAL(5, 2) NOT NULL COMMENT 'Discount percentage (e.g., 15.50)',
    StartDate DATE NOT NULL,
    EndDate DATE NOT NULL,
    MaxAllowedDiscount DECIMAL(5, 2) COMMENT 'Maximum allowed discount percentage',
    FOREIGN KEY (ProductId) REFERENCES Products(ProductId),
    CHECK (DiscountPercentage <= MaxAllowedDiscount OR MaxAllowedDiscount IS NULL)
);
CREATE TABLE Roles (
    RoleId INT AUTO_INCREMENT PRIMARY KEY,
    RoleName VARCHAR(50) NOT NULL
);

CREATE TABLE Users (
    UserId INT AUTO_INCREMENT PRIMARY KEY,
    Username VARCHAR(50) NOT NULL,
    PasswordHash VARCHAR(255) NOT NULL,
    RoleId INT,
    FOREIGN KEY (RoleId) REFERENCES Roles(RoleId) ON DELETE SET NULL
);
CREATE TABLE Orders (
    OrderId INT PRIMARY KEY AUTO_INCREMENT,
    UserId INT NOT NULL,
    OrderDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Status VARCHAR(50) NOT NULL DEFAULT 'Новый',
    FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE
);

CREATE TABLE OrderItems (
    OrderItemId INT PRIMARY KEY AUTO_INCREMENT,
    OrderId INT NOT NULL,
    ProductId INT NOT NULL,
    Quantity INT NOT NULL CHECK (Quantity > 0),
    FOREIGN KEY (OrderId) REFERENCES Orders(OrderId) ON DELETE CASCADE,
    FOREIGN KEY (ProductId) REFERENCES Products(ProductId)
);
