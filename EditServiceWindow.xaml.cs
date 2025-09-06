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

namespace BeautySalon
{
    /// <summary>
    /// Логика взаимодействия для EditServiceWindow.xaml
    /// </summary>
    public partial class EditServiceWindow : Window
    {
        private Service _service;
        private bool _isEditMode;

        public EditServiceWindow()
        {
            InitializeComponent();
            _isEditMode = false;
            Title = "Добавление услуги";
        }

        public EditServiceWindow(Service service)
        {
            InitializeComponent();
            _service = service;
            _isEditMode = true;
            Title = "Редактирование услуги";
            LoadServiceData();
        }

        private void LoadServiceData()
        {
            if (_service != null)
            {
                TitleTextBox.Text = _service.Title;
                CostTextBox.Text = _service.Cost.ToString();
                DurationTextBox.Text = (_service.DurationInSeconds / 60).ToString();
                DiscountTextBox.Text = _service.Discount?.ToString() ?? "0";
                DescriptionTextBox.Text = _service.Description;
                ImagePathTextBox.Text = _service.MainImagePath;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateData())
            {
                try
                {
                    using (var context = new BeautySalonEntityModel())
                    {
                        Service serviceToSave;

                        if (_isEditMode)
                        {
                            serviceToSave = context.Service.Find(_service.ID);
                        }
                        else
                        {
                            serviceToSave = new Service();
                            context.Service.Add(serviceToSave);
                        }

                        serviceToSave.Title = TitleTextBox.Text;
                        serviceToSave.Cost = decimal.Parse(CostTextBox.Text);
                        serviceToSave.DurationInSeconds = int.Parse(DurationTextBox.Text) * 60;

                        if (double.TryParse(DiscountTextBox.Text, out double discount))
                        {
                            serviceToSave.Discount = discount;
                        }
                        else
                        {
                            serviceToSave.Discount = 0;
                        }

                        serviceToSave.Description = DescriptionTextBox.Text;
                        serviceToSave.MainImagePath = ImagePathTextBox.Text;

                        context.SaveChanges();
                        DialogResult = true;
                        Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка сохранения: {ex.Message}");
                }
            }
        }

        private bool ValidateData()
        {
            // Простая валидация данных
            if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
            {
                MessageBox.Show("Введите название услуги");
                return false;
            }

            if (!decimal.TryParse(CostTextBox.Text, out decimal cost) || cost <= 0)
            {
                MessageBox.Show("Введите корректную стоимость");
                return false;
            }

            if (!int.TryParse(DurationTextBox.Text, out int duration) || duration <= 0)
            {
                MessageBox.Show("Введите корректную продолжительность");
                return false;
            }

            return true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
