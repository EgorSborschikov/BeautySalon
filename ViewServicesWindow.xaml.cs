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
    /// Логика взаимодействия для ViewServicesWindow.xaml
    /// </summary>
    public partial class ViewServicesWindow : Window
    {
        private BeautySalonEntityModel _context;
        private List<Service> _allServices;

        public ViewServicesWindow()
        {
            InitializeComponent();
            _context = new BeautySalonEntityModel();
            Loaded += ViewServicesWindow_Loaded;
        }

        private void LoadServices()
        {
            try
            {
                _allServices = _context.Service.ToList();

                foreach(var service in _allServices)
                {
                    if (!string.IsNullOrEmpty(service.MainImagePath))
                    {
                        service.MainImagePath = "/" + service.MainImagePath;
                    }
                }

                ApplyFiltersAndSort();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        private void ApplyFiltersAndSort()
        {
            if (_allServices == null) return;

            var filteredServices = _allServices.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchTextBox.Text))
            {
                string searchText = SearchTextBox.Text.ToLower();
                filteredServices = filteredServices.Where(s => s.Title.ToLower().Contains(searchText));
            }

            switch (FilterComboBox.SelectedIndex)
            {
                case 1:
                    filteredServices = filteredServices.Where(s => s.Discount.HasValue &&
                                                         s.Discount >= 0.00 &&
                                                         s.Discount <= 0.05);
                    break;
                case 2: 
                    filteredServices = filteredServices.Where(s => s.Discount.HasValue &&
                                                                 s.Discount > 0.05 &&
                                                                 s.Discount <= 0.15);
                    break;
                case 3: 
                    filteredServices = filteredServices.Where(s => s.Discount.HasValue &&
                                                                 s.Discount > 0.15 &&
                                                                 s.Discount <= 0.30);
                    break;
                case 4: 
                    filteredServices = filteredServices.Where(s => s.Discount.HasValue &&
                                                                 s.Discount > 0.30 &&
                                                                 s.Discount <= 0.70);
                    break;
                case 5: 
                    filteredServices = filteredServices.Where(s => s.Discount.HasValue &&
                                                                 s.Discount > 0.70 &&
                                                                 s.Discount <= 1.00);
                    break;
                case 0: 
                default:
                    break;
            }

            switch (SortComboBox.SelectedIndex)
            {
                case 1: 
                    filteredServices = filteredServices.OrderBy(s => s.Cost);
                    break;
                case 2: 
                    filteredServices = filteredServices.OrderByDescending(s => s.Cost);
                    break;
                default: 
                    filteredServices = filteredServices.OrderBy(s => s.ID);
                    break;
            }

            ServicesListBox.ItemsSource = filteredServices.ToList();
        }

        private void ViewServicesWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadServices();
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFiltersAndSort();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFiltersAndSort(); 
        }

        private void FilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFiltersAndSort();
        }
    }
}
