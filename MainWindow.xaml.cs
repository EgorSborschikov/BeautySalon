using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BeautySalon
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            if (PasswordTextBox.Text == "0000")
            {
                var adminPanelWindow = new AdminPanelWindow();
                adminPanelWindow.Show();
            } else
            {
                ShowError("Ошибка авторизации", "Введенный пароль неверный. Введенный вами пароль оказался неверным. Доступ к админ-панели недоступен.");
            }
        }

        private void BtnGuest_Click(object sender, RoutedEventArgs e)
        {
            var viewServicesWindow = new ViewServicesWindow();
            viewServicesWindow.Show();
        }

        private void ShowError(string title, string message)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
