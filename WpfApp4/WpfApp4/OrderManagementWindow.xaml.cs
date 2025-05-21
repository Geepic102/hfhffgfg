using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using WpfApp4.Entities;

namespace ProductApp
{
    public partial class OrderManagementWindow : Window
    {
        public ObservableCollection<Order> Orders { get; set; } = new ObservableCollection<Order>();

        public OrderManagementWindow()
        {
            InitializeComponent();
            LoadOrders();
            DataContext = this;
        }

        private void LoadOrders()
        {
            Orders.Clear();
            using (var db = new AppDbContext())
            {
                var orders = db.GetAllOrdersWithDetails(); // обязательно, чтобы возвращались Username и Items
                foreach (var order in orders)
                {
                    Orders.Add(order);
                }
            }
        }

        private void StatusComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox &&
                comboBox.DataContext is Order order &&
                comboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string newStatus = selectedItem.Content.ToString();

                if (!string.IsNullOrEmpty(newStatus) && newStatus != order.Status)
                {
                    order.Status = newStatus;

                    using (var db = new AppDbContext())
                    {
                        db.UpdateOrderStatus(order.OrderId, newStatus);
                    }
                }
            }
        }

        private void DeleteOrderButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Order order)
            {
                if (MessageBox.Show("Вы уверены, что хотите удалить заказ?", "Удаление", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    using (var db = new AppDbContext())
                    {
                        db.DeleteOrder(order.OrderId);
                    }
                    LoadOrders();
                }
            }
        }
    }
}
