using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
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
        ClientConnection cc;
        TcpClient client;
        string token;

        public ChangePasswordPanel(ClientConnection cc, TcpClient client, string token)
        {
            InitializeComponent();
            this.cc = cc;
            this.client = client;
            this.token = token;
        }

        private void changeButton_Click(object sender, RoutedEventArgs e)
        {
            if(passwordBox.Password.Equals(passwordBox2.Password))
            {
                cc.sendMessage(ClientCom.ChangePasswordMessage(passwordBox.Password, token), client);
                cc.readMessage(client);
                this.Close();
            }
        }
    }
}
