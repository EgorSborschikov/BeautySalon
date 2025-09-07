using BeautySalon.AdminWindows;
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
    /// Логика взаимодействия для AdminPanelWindow.xaml
    /// </summary>
    public partial class AdminPanelWindow : Window
    {
        public AdminPanelWindow()
        {
            InitializeComponent();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnServices_Click(object sender, RoutedEventArgs e)
        {
            var serviceEditorWindow = new ServiceEditorWindow();
            serviceEditorWindow.Show();
        }

        private void BtnImages_Click(object sender, RoutedEventArgs e)
        {
            var imageManagementWindow = new ImagesManagementWindow();
            imageManagementWindow.Show();
        }

        private void BtnClientRecords_Click(object sender, RoutedEventArgs e)
        {
            var clientRecording = new ClientRecordingWindow();
            clientRecording.Show();
        }

        private void BtnManageRecords_Click(object sender, RoutedEventArgs e)
        {
            var recordsManagement = new RecordsManagementWindow();
            recordsManagement.Show();
        }
    }
}
