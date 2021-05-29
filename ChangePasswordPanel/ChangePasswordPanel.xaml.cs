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

namespace LocalDatabase_Client.ChangePasswordPanel
{
    /// <summary>
    /// Logika interakcji dla klasy ChangePasswordPanel.xaml
    /// </summary>
    public partial class ChangePasswordPanel : Window
    {
        public ChangePasswordPanel()
        {
            InitializeComponent();
        }

        private void changePasswordButton(object sender, RoutedEventArgs e)
        {
            if(passwordBox.Password.Equals(passwordBox2.Password))
            {

            }
        }
    }
}
