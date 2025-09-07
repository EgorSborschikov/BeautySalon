using BeautySalon.Entities;
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
using System.Windows.Shapes;

namespace BeautySalon.AdminWindows
{
    /// <summary>
    /// Логика взаимодействия для ServiceEditorWindow.xaml
    /// </summary>
    public partial class ServiceEditorWindow : Window
    {
        private BeautySalonEntityModel _context;
        private List<Service> _services;

        public ServiceEditorWindow()
        {
            InitializeComponent();
            Loaded += ServicesEditorPage_Loaded;
        }

        private void ServicesEditorPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadServices();
        }

        private void LoadServices()
        {
            try
            {
                _context = new BeautySalonEntityModel();

                // Проверяем подключение к базе
                if (_context.Database.Exists())
                {
                    // Загружаем данные с включением отслеживания
                    _services = _context.Service.AsNoTracking().ToList();

                    MessageBox.Show($"Загружено услуг: {_services.Count}"); // Отладочное сообщение

                    // Обновляем пути к изображениям
                    foreach (var service in _services)
                    {
                        if (!string.IsNullOrEmpty(service.MainImagePath))
                        {
                            // Проверяем формат пути к изображению
                            if (!service.MainImagePath.StartsWith("/Resources/"))
                            {
                                service.MainImagePath = "/" + service.MainImagePath;
                            }
                        }
                        else
                        {
                            // Устанавливаем изображение по умолчанию, если путь пустой
                            service.MainImagePath = "/Resources/default_service.png";
                        }
                    }

                    ServicesListBox.ItemsSource = _services;

                    // Проверяем, есть ли данные в ListBox
                    if (ServicesListBox.Items.Count == 0)
                    {
                        MessageBox.Show("Нет данных для отображения");
                    }
                }
                else
                {
                    MessageBox.Show("Нет подключения к базе данных");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}\n\n{ex.InnerException?.Message}");
            }
        }

        private void ServicesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool isServiceSelected = ServicesListBox.SelectedItem != null;
            EditServiceButton.IsEnabled = isServiceSelected;
            DeleteServiceButton.IsEnabled = isServiceSelected;
        }

        private void AddServiceButton_Click(object sender, RoutedEventArgs e)
        {
            // Открываем окно добавления новой услуги
            var addServiceWindow = new EditServiceWindow();
            if (addServiceWindow.ShowDialog() == true)
            {
                // Обновляем список после добавления
                LoadServices();
            }
        }

        private void EditServiceButton_Click(object sender, RoutedEventArgs e)
        {
            if (ServicesListBox.SelectedItem is Service selectedService)
            {
                // Открываем окно редактирования услуги
                var editServiceWindow = new EditServiceWindow(selectedService);
                if (editServiceWindow.ShowDialog() == true)
                {
                    // Обновляем список после редактирования
                    LoadServices();
                }
            }
        }

        private void DeleteServiceButton_Click(object sender, RoutedEventArgs e)
        {
            if (ServicesListBox.SelectedItem is Service selectedService)
            {
                var result = MessageBox.Show($"Вы уверены, что хотите удалить услугу \"{selectedService.Title}\"?",
                                           "Подтверждение удаления",
                                           MessageBoxButton.YesNo,
                                           MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // Удаляем услугу из базы данных
                        var serviceToDelete = _context.Service.Find(selectedService.ID);
                        if (serviceToDelete != null)
                        {
                            _context.Service.Remove(serviceToDelete);
                            _context.SaveChanges();
                            LoadServices(); // Обновляем список
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении: {ex.Message}");
                    }
                }
            }
        }
    }
}
