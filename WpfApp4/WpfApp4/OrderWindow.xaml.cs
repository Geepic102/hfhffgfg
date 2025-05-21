using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using WpfApp4.Entities;

namespace ProductApp
{
    public partial class OrderWindow : Window
    {
        private readonly User _currentUser;
        private ObservableCollection<Product> _products;
        private ObservableCollection<OrderItem> _orderItems;

        public OrderWindow(User currentUser)
        {
            InitializeComponent();
            _currentUser = currentUser;
            _orderItems = new ObservableCollection<OrderItem>();
            LoadProducts();
        }

        private void LoadProducts()
        {
            using (var db = new AppDbContext())
            {
                _products = new ObservableCollection<Product>(db.GetProducts());
                ProductComboBox.ItemsSource = _products;
            }
        }

        private void AddProductButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProductComboBox.SelectedItem is Product selectedProduct &&
                int.TryParse(QuantityTextBox.Text, out int quantity) && quantity > 0)
            {
                if (quantity > selectedProduct.StockQuantity)
                {
                    MessageBox.Show($"Недостаточно товара на складе. Доступно: {selectedProduct.StockQuantity}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var existingItem = _orderItems.FirstOrDefault(i => i.ProductId == selectedProduct.ProductId);
                if (existingItem != null)
                {
                    int newTotalQuantity = existingItem.Quantity + quantity;
                    if (newTotalQuantity > selectedProduct.StockQuantity)
                    {
                        MessageBox.Show($"Общее количество превышает доступный остаток. Доступно: {selectedProduct.StockQuantity}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    existingItem.Quantity = newTotalQuantity;
                }
                else
                {
                    _orderItems.Add(new OrderItem
                    {
                        ProductId = selectedProduct.ProductId,
                        Product = selectedProduct,
                        Quantity = quantity
                    });
                }

                OrderListView.ItemsSource = null;
                OrderListView.ItemsSource = _orderItems;
            }
            else
            {
                MessageBox.Show("Выберите товар и укажите корректное количество.");
            }
        }



        private void CompleteOrderButton_Click(object sender, RoutedEventArgs e)
        {
            if (_orderItems.Count == 0)
            {
                MessageBox.Show("Сначала добавьте товары в заказ.");
                return;
            }

            using (var db = new AppDbContext())
            {
                var newOrder = new Order
                {
                    UserId = _currentUser.UserId,
                    OrderDate = DateTime.Now,
                    Status = "Новый",
                    Items = new ObservableCollection<OrderItem>(_orderItems)
                };

                db.AddOrder(newOrder);
                MessageBox.Show("Заказ успешно оформлен!");
                DialogResult = true;
                Close();
            }
        }
    }
}
