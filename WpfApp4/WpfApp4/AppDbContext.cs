using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Data;
using System.IO;
using MySql.Data.MySqlClient;
using WpfApp4.Entities;

namespace ProductApp
{
    public class AppDbContext : IDisposable
    {
        private readonly string connectionString = "user=root;password=123456789;port=3306;server=localhost;database=magazin";
        private readonly MySqlConnection _connection;
        private bool _disposed = false;

        public AppDbContext()
        {
            _connection = new MySqlConnection(connectionString);
            _connection.Open();
        }

        public ObservableCollection<Product> GetProducts()
        {
            var productList = new ObservableCollection<Product>();
            string imageFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");
            string defaultImage = Path.Combine(imageFolder, "picture.png");

            string query = @"SELECT p.ProductId, p.Article, p.Name, p.Description, p.ImageUrl, p.StockQuantity, 
                           pd.ManufacturerId, pd.SupplierId, pd.CategoryId, pd.Unit, pd.Price, 
                           d.DiscountId, d.DiscountPercentage, d.StartDate, d.EndDate, d.MaxAllowedDiscount, 
                           c.Name as CategoryName,
                           m.Name as ManufacturerName,
                           s.Name as SupplierName
                           FROM Products p
                           LEFT JOIN ProductDetails pd ON p.ProductId = pd.ProductId
                           LEFT JOIN Discounts d ON p.ProductId = d.ProductId
                           LEFT JOIN Categories c ON pd.CategoryId = c.CategoryId
                           LEFT JOIN Manufacturers m ON pd.ManufacturerId = m.ManufacturerId
                           LEFT JOIN Suppliers s ON pd.SupplierId = s.SupplierId";

            using var command = new MySqlCommand(query, _connection);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                string imagePath = Path.Combine(imageFolder, reader["ImageUrl"]?.ToString() ?? "picture.png");
                if (!File.Exists(imagePath)) imagePath = defaultImage;

                var product = new Product
                {
                    ProductId = Convert.ToInt32(reader["ProductId"]),
                    Article = reader["Article"].ToString(),
                    Name = reader["Name"].ToString(),
                    Description = reader["Description"].ToString(),
                    ImageUrl = imagePath,
                    StockQuantity = Convert.ToInt32(reader["StockQuantity"]),
                    ManufacturerId = reader["ManufacturerId"] as int?,
                    SupplierId = reader["SupplierId"] as int?,
                    CategoryId = reader["CategoryId"] as int?,
                    Unit = reader["Unit"].ToString(),
                    Price = Convert.ToDecimal(reader["Price"]),
                    Category = new Category { Name = reader["CategoryName"].ToString() },
                    Manufacturer = reader["ManufacturerName"] != DBNull.Value ? new Manufacturer { Name = reader["ManufacturerName"].ToString() } : null,
                    Supplier = reader["SupplierName"] != DBNull.Value ? new Supplier { Name = reader["SupplierName"].ToString() } : null,
                    Discount = reader["DiscountId"] != DBNull.Value ? new Discount
                    {
                        DiscountId = Convert.ToInt32(reader["DiscountId"]),
                        DiscountPercentage = Convert.ToDecimal(reader["DiscountPercentage"]),
                        StartDate = Convert.ToDateTime(reader["StartDate"]),
                        EndDate = Convert.ToDateTime(reader["EndDate"]),
                        MaxAllowedDiscount = reader["MaxAllowedDiscount"] as decimal?
                    } : null
                };
                productList.Add(product);
            }
            return productList;
        }

        public List<Category> GetCategories() => ExecuteSimpleListQuery("SELECT CategoryId, Name FROM Categories", reader =>
            new Category { CategoryId = Convert.ToInt32(reader["CategoryId"]), Name = reader["Name"].ToString() });

        public List<Supplier> GetSuppliers() => ExecuteSimpleListQuery("SELECT SupplierId, Name FROM Suppliers", reader =>
            new Supplier { SupplierId = Convert.ToInt32(reader["SupplierId"]), Name = reader["Name"].ToString() });

        public List<Manufacturer> GetManufacturers() => ExecuteSimpleListQuery("SELECT ManufacturerId, Name FROM Manufacturers", reader =>
            new Manufacturer { ManufacturerId = Convert.ToInt32(reader["ManufacturerId"]), Name = reader["Name"].ToString() });

        private List<T> ExecuteSimpleListQuery<T>(string query, Func<MySqlDataReader, T> map)
        {
            var list = new List<T>();
            using var cmd = new MySqlCommand(query, _connection);
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) list.Add(map(reader));
            return list;
        }

        public bool AddProduct(Product product)
        {
            using var conn = new MySqlConnection(connectionString);
            conn.Open();
            using var transaction = conn.BeginTransaction();

            try
            {
                string insertProduct = @"INSERT INTO Products (Article, Name, Description, ImageUrl, StockQuantity)
                                        VALUES (@Article, @Name, @Description, @ImageUrl, @StockQuantity);
                                        SELECT LAST_INSERT_ID();";

                int productId;
                using (var cmd = new MySqlCommand(insertProduct, conn, transaction))
                {
                    cmd.Parameters.AddWithValue("@Article", product.Article);
                    cmd.Parameters.AddWithValue("@Name", product.Name);
                    cmd.Parameters.AddWithValue("@Description", product.Description ?? "");
                    cmd.Parameters.AddWithValue("@ImageUrl", product.ImageUrl ?? "");
                    cmd.Parameters.AddWithValue("@StockQuantity", product.StockQuantity);
                    productId = Convert.ToInt32(cmd.ExecuteScalar());
                }

                string insertDetails = @"INSERT INTO ProductDetails (ProductId, ManufacturerId, SupplierId, CategoryId, Unit, Price)
                                          VALUES (@ProductId, @ManufacturerId, @SupplierId, @CategoryId, @Unit, @Price)";

                using (var cmd = new MySqlCommand(insertDetails, conn, transaction))
                {
                    cmd.Parameters.AddWithValue("@ProductId", productId);
                    cmd.Parameters.AddWithValue("@ManufacturerId", product.ManufacturerId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@SupplierId", product.SupplierId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@CategoryId", product.CategoryId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Unit", product.Unit ?? "шт");
                    cmd.Parameters.AddWithValue("@Price", product.Price);
                    cmd.ExecuteNonQuery();
                }

                if (product.Discount != null)
                {
                    string insertDiscount = @"INSERT INTO Discounts (ProductId, DiscountPercentage, StartDate, EndDate, MaxAllowedDiscount)
                                               VALUES (@ProductId, @DiscountPercentage, @StartDate, @EndDate, @MaxAllowedDiscount)";
                    using (var cmd = new MySqlCommand(insertDiscount, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@ProductId", productId);
                        cmd.Parameters.AddWithValue("@DiscountPercentage", product.Discount.DiscountPercentage);
                        cmd.Parameters.AddWithValue("@StartDate", product.Discount.StartDate);
                        cmd.Parameters.AddWithValue("@EndDate", product.Discount.EndDate);
                        cmd.Parameters.AddWithValue("@MaxAllowedDiscount", product.Discount.MaxAllowedDiscount ?? (object)DBNull.Value);
                        cmd.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Console.WriteLine("Ошибка добавления продукта: " + ex.Message);
                return false;
            }
        }

        public void UpdateOrderStatus(int orderId, string status)
        {
            const string query = "UPDATE Orders SET Status = @Status WHERE OrderId = @OrderId";
            using var cmd = new MySqlCommand(query, _connection);
            cmd.Parameters.AddWithValue("@OrderId", orderId);
            cmd.Parameters.AddWithValue("@Status", status);
            cmd.ExecuteNonQuery();
        }

        public void DeleteOrder(int orderId)
        {
            const string query = "DELETE FROM Orders WHERE OrderId = @OrderId";
            using var cmd = new MySqlCommand(query, _connection);
            cmd.Parameters.AddWithValue("@OrderId", orderId);
            cmd.ExecuteNonQuery();
        }

        public void AddOrder(Order order)
        {
            using (var transaction = _connection.BeginTransaction())
            {
                try
                {
                    string insertOrderQuery = "INSERT INTO Orders (UserId, OrderDate, Status) VALUES (@userId, @date, @status); SELECT LAST_INSERT_ID();";

                    int orderId;
                    using (var cmd = new MySqlCommand(insertOrderQuery, _connection, transaction))
                    {
                        cmd.Parameters.AddWithValue("@userId", order.UserId);
                        cmd.Parameters.AddWithValue("@date", order.OrderDate);
                        cmd.Parameters.AddWithValue("@status", order.Status);
                        orderId = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    foreach (var item in order.Items)
                    {
                        string insertItemQuery = "INSERT INTO OrderItems (OrderId, ProductId, Quantity) VALUES (@orderId, @productId, @quantity)";
                        using (var cmd = new MySqlCommand(insertItemQuery, _connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@orderId", orderId);
                            cmd.Parameters.AddWithValue("@productId", item.ProductId);
                            cmd.Parameters.AddWithValue("@quantity", item.Quantity);
                            cmd.ExecuteNonQuery();
                        }

                        string updateStockQuery = "UPDATE Products SET StockQuantity = StockQuantity - @quantity WHERE ProductId = @productId AND StockQuantity >= @quantity";
                        using (var cmd = new MySqlCommand(updateStockQuery, _connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@productId", item.ProductId);
                            cmd.Parameters.AddWithValue("@quantity", item.Quantity);
                            int affectedRows = cmd.ExecuteNonQuery();

                            if (affectedRows == 0)
                            {
                                throw new Exception("Недостаточное количество товара на складе для: " + item.ProductId);
                            }
                        }
                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }



        public User AuthenticateUser(string username, string password)
        {
            const string query = "SELECT u.UserId, u.Username, r.RoleName FROM Users u LEFT JOIN Roles r ON u.RoleId = r.RoleId WHERE u.Username = @username AND u.PasswordHash = @passwordHash";
            using var cmd = new MySqlCommand(query, _connection);
            cmd.Parameters.AddWithValue("@username", username);
            cmd.Parameters.AddWithValue("@passwordHash", HashPassword(password));

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new User
                {
                    UserId = reader.GetInt32("UserId"),
                    Username = reader.GetString("Username"),
                    Role = new Role { Name = reader["RoleName"].ToString() }
                };
            }
            return null;
        }

        public bool UserExists(string username)
        {
            const string query = "SELECT COUNT(*) FROM Users WHERE Username = @username";
            using var cmd = new MySqlCommand(query, _connection);
            cmd.Parameters.AddWithValue("@username", username);
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        public bool RegisterUser(string username, string password, string roleName)
        {
            if (UserExists(username)) return false;

            const string insertQuery = @"INSERT INTO Users (Username, PasswordHash, RoleId)
                                         VALUES (@username, @passwordHash, (SELECT RoleId FROM Roles WHERE RoleName = @roleName))";

            using var cmd = new MySqlCommand(insertQuery, _connection);
            cmd.Parameters.AddWithValue("@username", username);
            cmd.Parameters.AddWithValue("@passwordHash", HashPassword(password));
            cmd.Parameters.AddWithValue("@roleName", roleName);
            return cmd.ExecuteNonQuery() > 0;
        }

        public User GetUserByUsername(string username)
        {
            const string query = "SELECT u.UserId, u.Username, r.RoleName FROM Users u LEFT JOIN Roles r ON u.RoleId = r.RoleId WHERE u.Username = @username";
            using var cmd = new MySqlCommand(query, _connection);
            cmd.Parameters.AddWithValue("@username", username);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new User
                {
                    UserId = Convert.ToInt32(reader["UserId"]),
                    Username = reader["Username"].ToString(),
                    Role = new Role { Name = reader["RoleName"].ToString() }
                };
            }
            return null;
        }
        public void UpdateProduct(Product updatedProduct)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        string updateProductQuery = @"UPDATE Products SET 
                    Name = @Name, 
                    Description = @Description, 
                    ImageUrl = @ImageUrl, 
                    StockQuantity = @StockQuantity 
                    WHERE ProductId = @ProductId";

                        using (var cmd = new MySqlCommand(updateProductQuery, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@Name", updatedProduct.Name);
                            cmd.Parameters.AddWithValue("@Description", updatedProduct.Description ?? "");
                            cmd.Parameters.AddWithValue("@ImageUrl", updatedProduct.ImageUrl ?? "");
                            cmd.Parameters.AddWithValue("@StockQuantity", updatedProduct.StockQuantity);
                            cmd.Parameters.AddWithValue("@ProductId", updatedProduct.ProductId);

                            cmd.ExecuteNonQuery();
                        }

                        string updateDetailsQuery = @"UPDATE ProductDetails SET 
                    CategoryId = @CategoryId, 
                    Price = @Price 
                    WHERE ProductId = @ProductId";

                        using (var cmd = new MySqlCommand(updateDetailsQuery, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@CategoryId", updatedProduct.CategoryId ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Price", updatedProduct.Price);
                            cmd.Parameters.AddWithValue("@ProductId", updatedProduct.ProductId);
                            cmd.ExecuteNonQuery();
                        }

                        if (updatedProduct.Discount != null)
                        {
                            string checkDiscountQuery = "SELECT COUNT(*) FROM Discounts WHERE ProductId = @ProductId";
                            int count;

                            using (var cmd = new MySqlCommand(checkDiscountQuery, connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@ProductId", updatedProduct.ProductId);
                                count = Convert.ToInt32(cmd.ExecuteScalar());
                            }

                            if (count > 0)
                            {
                                string updateDiscountQuery = @"UPDATE Discounts SET 
                            DiscountPercentage = @DiscountPercentage, 
                            MaxAllowedDiscount = @MaxAllowedDiscount 
                            WHERE ProductId = @ProductId";

                                using (var cmd = new MySqlCommand(updateDiscountQuery, connection, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@DiscountPercentage", updatedProduct.Discount.DiscountPercentage);
                                    cmd.Parameters.AddWithValue("@MaxAllowedDiscount", updatedProduct.Discount.MaxAllowedDiscount ?? (object)DBNull.Value);
                                    cmd.Parameters.AddWithValue("@ProductId", updatedProduct.ProductId);
                                    cmd.ExecuteNonQuery();
                                }
                            }
                            else
                            {
                                string insertDiscountQuery = @"INSERT INTO Discounts (ProductId, DiscountPercentage, StartDate, EndDate, MaxAllowedDiscount)
                            VALUES (@ProductId, @DiscountPercentage, @StartDate, @EndDate, @MaxAllowedDiscount)";

                                using (var cmd = new MySqlCommand(insertDiscountQuery, connection, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@ProductId", updatedProduct.ProductId);
                                    cmd.Parameters.AddWithValue("@DiscountPercentage", updatedProduct.Discount.DiscountPercentage);
                                    cmd.Parameters.AddWithValue("@StartDate", DateTime.Now);
                                    cmd.Parameters.AddWithValue("@EndDate", DateTime.Now.AddMonths(1));
                                    cmd.Parameters.AddWithValue("@MaxAllowedDiscount", updatedProduct.Discount.MaxAllowedDiscount ?? (object)DBNull.Value);
                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
        public void DeleteProduct(int productId)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        string deleteDiscountQuery = "DELETE FROM Discounts WHERE ProductId = @ProductId";
                        using (var cmd = new MySqlCommand(deleteDiscountQuery, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@ProductId", productId);
                            cmd.ExecuteNonQuery();
                        }

                        string deleteDetailsQuery = "DELETE FROM ProductDetails WHERE ProductId = @ProductId";
                        using (var cmd = new MySqlCommand(deleteDetailsQuery, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@ProductId", productId);
                            cmd.ExecuteNonQuery();
                        }

                        string deleteProductQuery = "DELETE FROM Products WHERE ProductId = @ProductId";
                        using (var cmd = new MySqlCommand(deleteProductQuery, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@ProductId", productId);
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
        public List<Order> GetAllOrdersWithDetails()
        {
            var orders = new List<Order>();

            string query = @"
        SELECT o.OrderId, o.UserId, o.OrderDate, o.Status,
       u.Username,
       oi.ProductId, oi.Quantity,
       p.Name AS ProductName,
       pd.Price
FROM Orders o
LEFT JOIN Users u ON o.UserId = u.UserId
LEFT JOIN OrderItems oi ON o.OrderId = oi.OrderId
LEFT JOIN Products p ON oi.ProductId = p.ProductId
LEFT JOIN ProductDetails pd ON p.ProductId = pd.ProductId
ORDER BY o.OrderDate DESC;
";

            var orderDict = new Dictionary<int, Order>();

            using (var cmd = new MySqlCommand(query, _connection))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    int orderId = Convert.ToInt32(reader["OrderId"]);

                    if (!orderDict.TryGetValue(orderId, out Order order))
                    {
                        order = new Order
                        {
                            OrderId = orderId,
                            UserId = Convert.ToInt32(reader["UserId"]),
                            Username = reader["Username"].ToString(), // <--
                            OrderDate = Convert.ToDateTime(reader["OrderDate"]),
                            Status = reader["Status"].ToString(),
                            Items = new ObservableCollection<OrderItem>()
                        };

                        orderDict[orderId] = order;
                    }

                    if (reader["ProductId"] != DBNull.Value)
                    {
                        order.Items.Add(new OrderItem
                        {
                            ProductId = Convert.ToInt32(reader["ProductId"]),
                            Quantity = Convert.ToInt32(reader["Quantity"]),
                            ProductName = reader["ProductName"].ToString()
                        });
                    }
                }
            }

            return orderDict.Values.ToList();
        }
        public List<Order> GetOrdersByUserId(int userId)
        {
            var orders = new List<Order>();

            string query = @"
SELECT o.OrderId, o.UserId, o.OrderDate, o.Status,
       oi.OrderItemId, oi.ProductId, oi.Quantity,
       p.Name, pd.Price
FROM Orders o
LEFT JOIN OrderItems oi ON o.OrderId = oi.OrderId
LEFT JOIN Products p ON oi.ProductId = p.ProductId
LEFT JOIN ProductDetails pd ON p.ProductId = pd.ProductId
WHERE o.UserId = @userId
ORDER BY o.OrderDate DESC;";

            using (var cmd = new MySqlCommand(query, _connection))
            {
                cmd.Parameters.AddWithValue("@userId", userId);

                using (var reader = cmd.ExecuteReader())
                {
                    var orderDict = new Dictionary<int, Order>();

                    while (reader.Read())
                    {
                        int orderId = Convert.ToInt32(reader["OrderId"]);
                        if (!orderDict.TryGetValue(orderId, out Order order))
                        {
                            order = new Order
                            {
                                OrderId = orderId,
                                UserId = userId,
                                OrderDate = Convert.ToDateTime(reader["OrderDate"]),
                                Status = reader["Status"].ToString(),
                                Items = new ObservableCollection<OrderItem>()
                            };
                            orderDict[orderId] = order;
                        }

                        if (reader["ProductId"] != DBNull.Value)
                        {
                            order.Items.Add(new OrderItem
                            {
                                ProductId = Convert.ToInt32(reader["ProductId"]),
                                Quantity = Convert.ToInt32(reader["Quantity"]),
                                Product = new Product
                                {
                                    Name = reader["Name"].ToString(),
                                    Price = Convert.ToDecimal(reader["Price"])
                                }
                            });
                        }
                    }

                    orders = orderDict.Values.ToList();
                }
            }

            return orders;
        }



        private string HashPassword(string password)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                if (_connection.State != ConnectionState.Closed)
                    _connection.Close();
                _connection.Dispose();
                _disposed = true;
            }
            GC.SuppressFinalize(this);
        }

        ~AppDbContext() => Dispose();
    }
}