using System;
using System.Collections.ObjectModel;

namespace WpfApp4.Entities
{
    public class Manufacturer
    {
        public int ManufacturerId { get; set; }
        public string Name { get; set; }
    }

    public class Supplier
    {
        public int SupplierId { get; set; }
        public string Name { get; set; }
    }

    public class Category
    {
        public int CategoryId { get; set; }
        public string Name { get; set; }
    }

    public class Product
    {
        public int ProductId { get; set; }
        public string Article { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public int StockQuantity { get; set; }

        public int? ManufacturerId { get; set; }
        public Manufacturer Manufacturer { get; set; }

        public int? SupplierId { get; set; }
        public Supplier Supplier { get; set; }

        public int? CategoryId { get; set; }
        public Category Category { get; set; }

        public string Unit { get; set; }
        public decimal Price { get; set; }
       
        public Discount Discount { get; set; }
    }

    public class Discount
    {
        public int DiscountId { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public decimal DiscountPercentage { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal? MaxAllowedDiscount { get; set; }
    }
    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public Role Role { get; set; }
    }

    public class Role
    {
        public int RoleId { get; set; }
        public string Name { get; set; }
    }
    public class Order
    {
        public int OrderId { get; set; }
        public string Username { get; set; } 
        public int TotalQuantity => Items != null ? Items.Sum(i => i.Quantity) : 0;

        public int UserId { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = "Новый";
        public ObservableCollection<OrderItem> Items { get; set; } = new ObservableCollection<OrderItem>();

    }



    public class OrderItem
    {
        public int OrderItemId { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public Product Product { get; set; }

        public string ProductName { get; set; } 
    }

}
