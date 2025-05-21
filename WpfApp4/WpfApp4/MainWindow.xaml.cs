using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WpfApp4;
using WpfApp4.Entities;

namespace ProductApp
{
    public partial class ProductListWindow : Window, INotifyPropertyChanged
    {
        public ObservableCollection<Product> Products { get; set; } = new ObservableCollection<Product>();
        private ObservableCollection<Product> AllProducts { get; set; } = new ObservableCollection<Product>();
        private User CurrentUser { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public ProductListWindow()
        {
            InitializeComponent();
            LoadDataFromDatabase();
            DataContext = this;
            UpdateUserInterface();
        }

        private bool IsAdmin => CurrentUser?.Role?.Name == "Администратор";
        private bool IsClient => CurrentUser?.Role?.Name == "Клиент";

        public Visibility EditButtonVisibility => IsAdmin ? Visibility.Visible : Visibility.Collapsed;
        public Visibility AddButtonVisibility => IsAdmin ? Visibility.Visible : Visibility.Collapsed;
        public Visibility ManageOrdersVisibility => IsAdmin ? Visibility.Visible : Visibility.Collapsed;
        public Visibility OrderButtonVisibility => IsClient ? Visibility.Visible : Visibility.Collapsed;

        public Visibility RegisterVisibility => CurrentUser == null ? Visibility.Visible : Visibility.Collapsed;
        public Visibility LoginVisibility => CurrentUser == null ? Visibility.Visible : Visibility.Collapsed;
        public Visibility LogoutVisibility => CurrentUser != null ? Visibility.Visible : Visibility.Collapsed;

        private void LoadDataFromDatabase()
        {
            using (AppDbContext dbContext = new AppDbContext())
            {
                var loadedProducts = dbContext.GetProducts();
                if (loadedProducts != null)
                {
                    AllProducts = loadedProducts;
                    ApplyFilters();
                }
            }
        }

        private void EditProductButton_Click(object sender, RoutedEventArgs e)
        {
            if (!IsAdmin)
            {
                MessageBox.Show("Только администратор может редактировать товары.", "Отказано", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var button = sender as Button;
            var product = button?.DataContext as Product;

            if (product != null)
            {
                var editWindow = new EditProductWindow(product.ProductId);
                if (editWindow.ShowDialog() == true)
                {
                    LoadDataFromDatabase();
                }
            }
        }

        private void UpdateUserInterface()
        {
            if (CurrentUser != null)
            {
                UserInfoText.Text = $"{CurrentUser.Username} ({CurrentUser.Role.Name})";
            }
            else
            {
                UserInfoText.Text = string.Empty;
            }

            OnPropertyChanged(nameof(EditButtonVisibility));
            OnPropertyChanged(nameof(AddButtonVisibility));
            OnPropertyChanged(nameof(ManageOrdersVisibility));
            OnPropertyChanged(nameof(OrderButtonVisibility));
            OnPropertyChanged(nameof(RegisterVisibility));
            OnPropertyChanged(nameof(LoginVisibility));
            OnPropertyChanged(nameof(LogoutVisibility));
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new LoginWindow();
            if (loginWindow.ShowDialog() == true)
            {
                CurrentUser = loginWindow.AuthenticatedUser;
                MessageBox.Show($"Добро пожаловать, {CurrentUser.Username}!");
                UpdateUserInterface();
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            var registerWindow = new RegisterWindow();
            if (registerWindow.ShowDialog() == true)
            {
                MessageBox.Show("Регистрация прошла успешно! Теперь вы можете войти.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Вы действительно хотите выйти?", "Выход", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                CurrentUser = null;
                UpdateUserInterface();
                Products.Clear();
                LoadDataFromDatabase();
            }
        }

        private void DiscountFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplyFilters();

        private void SearchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchTextBox.Text == "Найти")
            {
                SearchTextBox.Text = "";
                SearchTextBox.Foreground = Brushes.Black;
            }
        }

        private void SearchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchTextBox.Text))
            {
                SearchTextBox.Text = "Найти";
                SearchTextBox.Foreground = Brushes.Gray;
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilters();

        private void ApplyFilters()
        {
            string discountTag = (DiscountFilterComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString();
            string searchText = SearchTextBox.Text?.ToLower() ?? "";

            var filtered = AllProducts.AsEnumerable();

            switch (discountTag)
            {
                case "0-3":
                    filtered = filtered.Where(p => p.Discount != null && p.Discount.DiscountPercentage >= 0 && p.Discount.DiscountPercentage <= 3);
                    break;
                case "4-5":
                    filtered = filtered.Where(p => p.Discount != null && p.Discount.DiscountPercentage >= 4 && p.Discount.DiscountPercentage <= 5);
                    break;
                case "6+":
                    filtered = filtered.Where(p => p.Discount != null && p.Discount.DiscountPercentage >= 6);
                    break;
            }

            if (!string.IsNullOrWhiteSpace(searchText) && searchText != "найти")
            {
                filtered = filtered.Where(p => p.Name != null && p.Name.ToLower().Contains(searchText));
            }

            Products.Clear();
            foreach (var product in filtered)
            {
                Products.Add(product);
            }

            UpdateProductCount();
        }

        private void UpdateProductCount() =>
            ProductsCountText.Text = $"Товаров найдено: {Products.Count}";

        private void AddProductWindow_Click(object sender, RoutedEventArgs e)
        {
            if (!IsAdmin)
            {
                MessageBox.Show("Добавление товаров доступно только для администратора.");
                return;
            }

            var addProductWindow = new AddProductWindow();
            if (addProductWindow.ShowDialog() == true)
            {
                LoadDataFromDatabase();
            }
        }

        private void OrderButton_Click(object sender, RoutedEventArgs e)
        {
            var orderWindow = new OrderWindow(CurrentUser);
            if (orderWindow.ShowDialog() == true)
            {
                LoadDataFromDatabase();
            }
        }
        private void ClientOrdersButton_Click(object sender, RoutedEventArgs e)
        {
            var userOrdersWindow = new UserOrdersWindow(CurrentUser); 
            userOrdersWindow.ShowDialog();
        }


        private void ManageOrdersButton_Click(object sender, RoutedEventArgs e)
        {
            if (!IsAdmin)
            {
                MessageBox.Show("Доступ только для администратора.");
                return;
            }

            var manageOrdersWindow = new OrderManagementWindow();
            manageOrdersWindow.ShowDialog();
        }
    }
}
