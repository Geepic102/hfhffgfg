using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using WpfApp4.Entities;

namespace ProductApp
{
    public partial class EditProductWindow : Window
    {
        private readonly AppDbContext _dbContext;
        private readonly int _productId;
        private string _newImagePath;
        private readonly string _imageFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");

        public EditProductWindow(int productId)
        {
            InitializeComponent();
            _dbContext = new AppDbContext();
            _productId = productId;
            LoadProductData();
        }

        private void LoadProductData()
        {
            var product = _dbContext.GetProducts().FirstOrDefault(p => p.ProductId == _productId);

            if (product != null)
            {
                ArticleTextBox.Text = product.Article;
                NameTextBox.Text = product.Name;
                DescriptionTextBox.Text = product.Description;
                PriceTextBox.Text = product.Price.ToString();
                QuantityTextBox.Text = product.StockQuantity.ToString();
                MaxDiscountTextBox.Text = product.Discount?.MaxAllowedDiscount.ToString() ?? "0";
                CurrentDiscountTextBox.Text = product.Discount?.DiscountPercentage.ToString() ?? "0";

                if (!string.IsNullOrEmpty(product.ImageUrl) && File.Exists(product.ImageUrl))
                {
                    ProductImage.Source = new BitmapImage(new Uri(product.ImageUrl));
                }

                CategoryComboBox.ItemsSource = _dbContext.GetCategories();
                CategoryComboBox.SelectedValue = product.CategoryId;
            }
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
            try
            {
                var product = _dbContext.GetProducts().FirstOrDefault(p => p.ProductId == _productId);

                if (product != null)
                {
                    product.Name = NameTextBox.Text;
                    product.Description = DescriptionTextBox.Text;
                    product.Price = decimal.Parse(PriceTextBox.Text);
                    product.StockQuantity = int.Parse(QuantityTextBox.Text);
                    product.CategoryId = (int)CategoryComboBox.SelectedValue;

                    if (!string.IsNullOrEmpty(_newImagePath))
                    {
                        var fileName = Path.GetFileName(_newImagePath);
                        var destPath = Path.Combine(_imageFolder, fileName);

                        if (!Directory.Exists(_imageFolder))
                        {
                            Directory.CreateDirectory(_imageFolder);
                        }

                        File.Copy(_newImagePath, destPath, overwrite: true);
                        product.ImageUrl = fileName;
                    }

                    if (product.Discount == null)
                    {
                        product.Discount = new Discount();
                    }

                    if (decimal.TryParse(MaxDiscountTextBox.Text, out decimal maxDiscount) &&
                        decimal.TryParse(CurrentDiscountTextBox.Text, out decimal currentDiscount))
                    {
                        product.Discount.MaxAllowedDiscount = maxDiscount;
                        product.Discount.DiscountPercentage = currentDiscount;
                    }

                    _dbContext.UpdateProduct(product);
                    DialogResult = true;
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите удалить этот товар?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _dbContext.DeleteProduct(_productId);
                    DialogResult = true;
                    Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}