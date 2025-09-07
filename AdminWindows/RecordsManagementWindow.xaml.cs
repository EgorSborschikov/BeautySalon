using BeautySalon.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
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
using System.Windows.Threading;

namespace BeautySalon.AdminWindows
{
    /// <summary>
    /// Логика взаимодействия для RecordsManagementWindow.xaml
    /// </summary>
    public partial class RecordsManagementWindow : Window
    {
        private BeautySalonEntityModel _context;
        private DispatcherTimer _refreshTimer;
        private DateTime _lastUpdateTime;


        public RecordsManagementWindow()
        {
            InitializeComponent();
            Loaded += RecordsManagementWindow_Loaded;
            Unloaded += RecordsManagementWindow_Unloaded;
        }

        private void RecordsManagementWindow_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeTimer();
            LoadRecords();
        }

        private void InitializeTimer()
        {
            _refreshTimer = new DispatcherTimer();
            _refreshTimer.Interval = TimeSpan.FromSeconds(30); // Обновление каждые 30 секунд
            _refreshTimer.Tick += RefreshTimer_Tick;
            _refreshTimer.Start();
        }

        private async void RefreshTimer_Tick(object sender, EventArgs e)
        {
            await LoadRecordsAsync();
        }

        private async void LoadRecords()
        {
            await LoadRecordsAsync();
        }

        private void RecordsManagementWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            // Останавливаем таймер при закрытии страницы
            if (_refreshTimer != null)
            {
                _refreshTimer.Stop();
                _refreshTimer = null;
            }
        }

        private async Task LoadRecordsAsync()
        {
            try
            {
                _context = new BeautySalonEntityModel();

                DateTime startDate = DateTime.Today;
                DateTime endDate = DateTime.Today.AddDays(2); // Сегодня + завтра

                // Определяем фильтр по дате
                switch (DateFilterComboBox.SelectedIndex)
                {
                    case 0: // Сегодня
                        endDate = DateTime.Today.AddDays(1).AddSeconds(-1);
                        break;
                    case 1: // Завтра
                        startDate = DateTime.Today.AddDays(1);
                        endDate = DateTime.Today.AddDays(2).AddSeconds(-1);
                        break;
                    case 2: // Все ближайшие
                        endDate = DateTime.Today.AddDays(7); // Неделя вперед
                        break;
                }

                var records = await _context.ClientService
                    .Where(cs => cs.StartTime >= startDate && cs.StartTime <= endDate)
                    .Include(cs => cs.Service)
                    .Include(cs => cs.Client)
                    .OrderBy(cs => cs.StartTime) // Сортировка по дате и времени
                    .ToListAsync();

                var recordViewModels = records.Select(cs => new RecordViewModel
                {
                    Id = cs.ID,
                    ServiceTitle = cs.Service.Title,
                    ClientFullName = $"{cs.Client.LastName} {cs.Client.FirstName} {cs.Client.Patronymic}".Trim(),
                    ClientEmail = cs.Client.Email,
                    ClientPhone = cs.Client.Phone,
                    StartTime = cs.StartTime,
                    DateTimeString = cs.StartTime.ToString("dd.MM.yyyy HH:mm"),
                    TimeRemaining = GetTimeRemainingString(cs.StartTime),
                    IsLessThanHour = (cs.StartTime - DateTime.Now).TotalHours < 1
                }).ToList();

                RecordsItemsControl.ItemsSource = recordViewModels;

                _lastUpdateTime = DateTime.Now;
                LastUpdateText.Text = $"Последнее обновление: {_lastUpdateTime:HH:mm:ss}";
                RecordsCountText.Text = $"Записей: {recordViewModels.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки записей: {ex.Message}");
            }
        }

        private string GetTimeRemainingString(DateTime startTime)
        {
            TimeSpan timeRemaining = startTime - DateTime.Now;

            if (timeRemaining.TotalSeconds <= 0)
                return "Запись завершена";

            if (timeRemaining.TotalDays >= 1)
                return $"{(int)timeRemaining.TotalDays} дн. {timeRemaining.Hours} час.";

            return $"{timeRemaining.Hours} час. {timeRemaining.Minutes} мин.";
        }

        private void DateFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadRecords();
        }

        private void RefreshBtn_Click(object sender, RoutedEventArgs e)
        {
            LoadRecords();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    public class RecordViewModel
    {
        public int Id { get; set; }
        public string ServiceTitle { get; set; }
        public string ClientFullName { get; set; }
        public string ClientEmail { get; set; }
        public string ClientPhone { get; set; }
        public DateTime StartTime { get; set; }
        public string DateTimeString { get; set; }
        public string TimeRemaining { get; set; }
        public bool IsLessThanHour { get; set; }
    }
}
