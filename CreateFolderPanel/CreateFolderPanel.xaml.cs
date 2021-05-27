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

namespace LocalDatabase_Client.CreateFolderPanel
{
    /// <summary>
    /// Logika interakcji dla klasy CreateFolderPanel.xaml
    /// </summary>
    public partial class CreateFolderPanel : Window
    {
        public string folderName;
        public CreateFolderPanel()
        {
            InitializeComponent();
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
        }

        private void createButton_Click(object sender, RoutedEventArgs e)
        {
            folderName = textBox.Text;
            this.Close();
        }
    }
}
