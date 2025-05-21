using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Media.Imaging;
using WpfApp4.Entities;

namespace ProductApp
{
    public partial class AddProductWindow : Window
    {
        private readonly AppDbContext _db = new AppDbContext();
        private string _newImagePath;

        public AddProductWindow()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {

            CategoryComboBox.ItemsSource = _db.GetCategories();
        }
        private void ChangeImageButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.jpg, *.jpeg, *.png)|*.jpg;*.jpeg;*.png",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _newImagePath = openFileDialog.FileName;
                ProductImage.Source = new BitmapImage(new Uri(_newImagePath));
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ArticleTextBox.Text) || string.IsNullOrWhiteSpace(NameTextBox.Text) ||
                string.IsNullOrWhiteSpace(QuantityTextBox.Text) || string.IsNullOrWhiteSpace(MaxDiscountTextBox.Text) ||
                string.IsNullOrWhiteSpace(CurrentDiscountTextBox.Text) || string.IsNullOrWhiteSpace(PriceTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, заполните все обязательные поля!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var discount = new Discount
            {
                DiscountPercentage = Convert.ToDecimal(CurrentDiscountTextBox.Text),
                MaxAllowedDiscount = Convert.ToDecimal(MaxDiscountTextBox.Text),
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddMonths(1)
            };

            var newProduct = new Product
            {
                Article = ArticleTextBox.Text,
                Name = NameTextBox.Text,
                Description = DescriptionTextBox.Text,
                ImageUrl = _newImagePath,
                StockQuantity = Convert.ToInt32(QuantityTextBox.Text),
                Price = Convert.ToDecimal(PriceTextBox.Text),
                CategoryId = (CategoryComboBox.SelectedItem as Category)?.CategoryId,
                SupplierId = null,
                ManufacturerId = null,
                Discount = discount
            };

            if (_db.AddProduct(newProduct))
            {
                MessageBox.Show("Товар успешно добавлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }

            else
            {
                MessageBox.Show("Ошибка при добавлении товара.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



    }
}
