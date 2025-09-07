using BeautySalon.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Логика взаимодействия для ClientRecordingWindow.xaml
    /// </summary>
    public partial class ClientRecordingWindow : Window
    {

        private BeautySalonEntityModel _context;
        private Service _selectedService;

        public ClientRecordingWindow()
        {
            InitializeComponent();
            Loaded += ClientRecordingWindow_Loaded;
        }

        private void ClientRecordingWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                _context = new BeautySalonEntityModel();

                var services = _context.Service.ToList();
                ServicesComboBox.ItemsSource = services;

                var clients = _context.Client.Select(c => new
                {
                    c.ID, 
                    FullName = c.LastName + " " + c.FirstName + " " + (c.Patronymic ?? ""),
                    c.FirstName,
                    c.LastName,
                    c.Patronymic
                }).ToList();

                ClientsComboBox.ItemsSource = clients;

                ServiceDatePicker.SelectedDate = DateTime.Today;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        private void ServicesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedService = ServicesComboBox.SelectedItem as Service;
            if (_selectedService != null)
            {
                DurationText.Text = (_selectedService.DurationInSeconds / 60).ToString();

                CalculateEndTime();
            }
        }

        private void ServiceDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            CalculateEndTime();
        }

        private void StartTimeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            CalculateEndTime();
        }

        private void StartTimeTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Разрешаем только цифры и двоеточие
            var regex = new Regex(@"^[0-9:]+$");
            e.Handled = !regex.IsMatch(e.Text);
        }

        private void CalculateEndTime()
        {
            if (_selectedService == null || string.IsNullOrEmpty(StartTimeTextBox.Text))
                return;

            try
            {
                // Парсим время начала
                var timeParts = StartTimeTextBox.Text.Split(':');
                if (timeParts.Length != 2)
                    return;

                if (int.TryParse(timeParts[0], out int hours) && int.TryParse(timeParts[1], out int minutes))
                {
                    if (hours < 0 || hours > 23 || minutes < 0 || minutes > 59)
                    {
                        EndTimeText.Text = "Неверное время";
                        return;
                    }

                    // Получаем выбранную дату
                    DateTime selectedDate = ServiceDatePicker.SelectedDate ?? DateTime.Today;
                    DateTime startTime = new DateTime(selectedDate.Year, selectedDate.Month, selectedDate.Day, hours, minutes, 0);

                    // Добавляем длительность услуги
                    DateTime endTime = startTime.AddSeconds(_selectedService.DurationInSeconds);

                    EndTimeText.Text = endTime.ToString("HH:mm");
                }
            }
            catch
            {
                EndTimeText.Text = "Ошибка расчета";
            }
        }

        private async void RecordClientButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput())
                return;

            try
            {
                var selectedClient = ClientsComboBox.SelectedItem as dynamic;
                if (selectedClient == null || _selectedService == null)
                    return;

                // Парсим время начала
                var timeParts = StartTimeTextBox.Text.Split(':');
                int hours = int.Parse(timeParts[0]);
                int minutes = int.Parse(timeParts[1]);

                DateTime selectedDate = ServiceDatePicker.SelectedDate ?? DateTime.Today;
                DateTime startTime = new DateTime(selectedDate.Year, selectedDate.Month, selectedDate.Day, hours, minutes, 0);
                DateTime endTime = startTime.AddSeconds(_selectedService.DurationInSeconds);

                // Проверяем нет ли пересечений по времени
                bool hasConflict = await _context.ClientService
                    .Where(cs => cs.ServiceID == _selectedService.ID)
                    .AnyAsync(cs => startTime < DbFunctions.AddSeconds(cs.StartTime, cs.Service.DurationInSeconds) &&
                                   endTime > cs.StartTime);

                if (hasConflict)
                {
                    ShowMessage("В это время уже есть запись на эту услугу");
                    return;
                }

                // Создаем новую запись
                var clientService = new ClientService
                {
                    ClientID = selectedClient.ID,
                    ServiceID = _selectedService.ID,
                    StartTime = startTime,
                    Comment = $"Запись создана {DateTime.Now:dd.MM.yyyy HH:mm}"
                };

                _context.ClientService.Add(clientService);
                await _context.SaveChangesAsync();

                ShowMessage("Клиент успешно записан на услугу!", isSuccess: true);
                ClearForm();
            }
            catch (Exception ex)
            {
                ShowMessage($"Ошибка при записи: {ex.Message}");
            }
        }

        private bool ValidateInput()
        {
            if (ServicesComboBox.SelectedItem == null)
            {
                ShowMessage("Выберите услугу");
                return false;
            }

            if (ClientsComboBox.SelectedItem == null)
            {
                ShowMessage("Выберите клиента");
                return false;
            }

            if (ServiceDatePicker.SelectedDate == null)
            {
                ShowMessage("Выберите дату");
                return false;
            }

            if (string.IsNullOrEmpty(StartTimeTextBox.Text) || !StartTimeTextBox.Text.Contains(":"))
            {
                ShowMessage("Введите время начала в формате HH:MM");
                return false;
            }

            // Проверяем корректность времени
            var timeParts = StartTimeTextBox.Text.Split(':');
            if (timeParts.Length != 2 ||
                !int.TryParse(timeParts[0], out int hours) ||
                !int.TryParse(timeParts[1], out int minutes) ||
                hours < 0 || hours > 23 || minutes < 0 || minutes > 59)
            {
                ShowMessage("Введите корректное время (00:00 - 23:59)");
                return false;
            }

            // Проверяем что время не в прошлом
            DateTime selectedDate = ServiceDatePicker.SelectedDate ?? DateTime.Today;
            DateTime selectedTime = new DateTime(selectedDate.Year, selectedDate.Month, selectedDate.Day, hours, minutes, 0);

            if (selectedTime < DateTime.Now)
            {
                ShowMessage("Нельзя записывать на прошедшее время");
                return false;
            }

            return true;
        }

        private void ShowMessage(string message, bool isSuccess = false)
        {
            MessageText.Text = message;
            MessageText.Foreground = isSuccess ? System.Windows.Media.Brushes.Green : System.Windows.Media.Brushes.Red;
        }

        private void ClearForm()
        {
            ServicesComboBox.SelectedIndex = -1;
            ClientsComboBox.SelectedIndex = -1;
            StartTimeTextBox.Text = "";
            EndTimeText.Text = "";
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
