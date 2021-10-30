using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;



namespace LocalDatabase_Client.Registration


{
    /// <summary>
    /// Logika interakcji dla klasy Registration.xaml
    /// </summary>
    public partial class Registration : Window
    {
        private SslStream sslStream;
        private ClientConnection cc;
        public Registration()
        {
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen; //app is always in center of screen
            InitializeComponent();
        }

        private void Connection()
        {
            cc = new ClientConnection("127.0.0.1");
            sslStream = cc.Start();
        }

        //click button event. If the name and surname are longer than 2 system add new user - > look at Database/DatabaseManager.cs => AddUser()
        //if the condition is not met then user has to try again.
        private void createButton_Click(object sender, RoutedEventArgs e)
        {


            MessagePanel.MessagePanel mp;
            
            if (surnameTextBox.Text.Length > 2 && nameTextBox.Text.Length > 2)
            {
               
                mp = new MessagePanel.MessagePanel("Registration success", false);
                mp.ShowDialog();
                this.Close();
            }
            else
            {
                mp = new MessagePanel.MessagePanel("Data is not valid", false);
                mp.ShowDialog();
            }
                
        }

        //if user clicks back button then panel just close.
        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
