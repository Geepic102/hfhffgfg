using System.Collections.ObjectModel;
using System.Windows;
using WpfApp4.Entities;

namespace ProductApp
{
    public partial class UserOrdersWindow : Window
    {
        public ObservableCollection<Order> UserOrders { get; set; } = new ObservableCollection<Order>();
        private readonly User _currentUser;

        public UserOrdersWindow(User currentUser)
        {
            InitializeComponent();
            _currentUser = currentUser;
            LoadOrders();
            DataContext = this;
        }

        private void LoadOrders()
        {
            using (var db = new AppDbContext())
            {
                var orders = db.GetOrdersByUserId(_currentUser.UserId);
                UserOrders.Clear();
                foreach (var order in orders)
                {
                    UserOrders.Add(order);
                }
            }
        }
    }
}
