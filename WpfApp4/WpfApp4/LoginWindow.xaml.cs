using MySql.Data.MySqlClient;
using ProductApp;
using System.Windows;
using WpfApp4.Entities;

namespace WpfApp4
{
    public partial class LoginWindow : Window
    {
        public User AuthenticatedUser { get; private set; }

        public LoginWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(username))
            {
                ShowErrorMessage("Введите имя пользователя");
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                ShowErrorMessage("Введите пароль");
                return;
            }

            try
            {
                var dbContext = new AppDbContext();
                AuthenticatedUser = dbContext.AuthenticateUser(username, password);

                if (AuthenticatedUser != null)
                {
                    DialogResult = true;
                    Close();
                }
                else
                {
                    ShowErrorMessage("Неверное имя пользователя или пароль");
                }
            }
            catch (MySqlException ex)
            {
                ShowErrorMessage($"Ошибка подключения к базе данных: {ex.Message}");
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Произошла ошибка: {ex.Message}");
            }
        }

        private void ShowErrorMessage(string message)
        {
            ErrorMessageText.Text = message;
            ErrorMessageText.Visibility = Visibility.Visible;
        }
    }
}