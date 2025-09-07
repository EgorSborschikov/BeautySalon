using BeautySalon.Entities;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.IO;
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
    /// Логика взаимодействия для ImagesManagementWindow.xaml
    /// </summary>
    public partial class ImagesManagementWindow : Window
    {
        private BeautySalonEntityModel _context;
        private Service _selectedService;

        public ImagesManagementWindow()
        {
            InitializeComponent();
            Loaded += ImagesManagementWindow_Loaded;
        }

        private void ImagesManagementWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadServices();
        }

        private void LoadServices() 
        {
            try
            {
                _context = new BeautySalonEntityModel();
                var service = _context.Service.ToList();
                ServicesComboBox.ItemsSource = service;

            } catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки услуг: {ex.Message}");
            }
        }

        private void ServicesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedService = ServicesComboBox.SelectedItem as Service;
            if (_selectedService != null)
            {
                SelectedServiceText.Text = _selectedService.Title;
                LoadServiceImages();
            }
        }

        private void LoadServiceImages() 
        {
            if (_selectedService == null) return;

            try
            {
                _context.Entry(_selectedService).Collection(s => s.ServicePhoto).Load();
                var images = _selectedService.ServicePhoto.ToList();

                foreach (var image in images)
                {
                    if (!string.IsNullOrEmpty(image.PhotoPath))
                    {
                        image.PhotoPath = "/Resources/ServicePhotos/" + System.IO.Path.GetFileName(image.PhotoPath);
                    }
                }

                ImagesItemsControl.ItemsSource = images;
            }
            catch (Exception ex) 
            {
                MessageBox.Show($"Ошибка загрузки изображений: {ex.Message}");
            }
        }

        private void AddImagesButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedService == null)
            {
                MessageBox.Show("Сначала выберите услугу");
                return;
            }

            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.jpg; *.jpeg; *.png; *.bmp)|*.jpg; *.jpeg; *.png; *.bmp",
                Multiselect = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string targetDirectory = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "ServicePhotos");

                    // Создаем директорию, если не существует
                    if (!Directory.Exists(targetDirectory))
                    {
                        Directory.CreateDirectory(targetDirectory);
                    }

                    foreach (var filePath in openFileDialog.FileNames)
                    {
                        string fileName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(filePath);
                        string targetPath = System.IO.Path.Combine(targetDirectory, fileName);

                        // Копируем файл в папку проекта
                        File.Copy(filePath, targetPath, true);

                        // Сохраняем в базу данных
                        var servicePhoto = new ServicePhoto
                        {
                            ServiceID = _selectedService.ID,
                            PhotoPath = fileName // Сохраняем только имя файла
                        };

                        _context.ServicePhoto.Add(servicePhoto);
                    }

                    _context.SaveChanges();
                    LoadServiceImages(); // Обновляем галерею

                    MessageBox.Show("Изображения успешно добавлены");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при добавлении изображений: {ex.Message}");
                }
            }
        }

        private void DeleteImageButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int imageId)
            {
                var result = MessageBox.Show("Удалить это изображение?", "Подтверждение",
                                           MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var imageToDelete = _context.ServicePhoto.Find(imageId);
                        if (imageToDelete != null)
                        {
                            // Удаляем файл изображения
                            string imagePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                                                          "Resources", "ServicePhotos", imageToDelete.PhotoPath);
                            if (File.Exists(imagePath))
                            {
                                File.Delete(imagePath);
                            }

                            // Удаляем запись из базы
                            _context.ServicePhoto.Remove(imageToDelete);
                            _context.SaveChanges();

                            LoadServiceImages(); // Обновляем галерею
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении изображения: {ex.Message}");
                    }
                }
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadServices();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
