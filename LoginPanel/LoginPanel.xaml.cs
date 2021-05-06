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

namespace LocalDatabase_Client.LoginPanel
{
    /// <summary>
    /// Logika interakcji dla klasy LoginPanel.xaml
    /// </summary>
    public partial class LoginPanel : Window
    {
        private TcpClient client;
        private ClientConnection cc;
        private bool isLogged = false;

        public LoginPanel()
        {
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            InitializeComponent();
            Task t = new Task(() => Connection());
            t.Start();
        }

        private void Connection()
        {
            cc = new ClientConnection("192.168.1.19");
            client = cc.Start();
        }

        private void loginButton_Click(object sender, RoutedEventArgs e)
        {
            if (client == null)
                MessageBox.Show("Błąd połączenia z serwerem");
            else
            {
                if (client.Connected)
                {
                    cc.sendMessage(ClientCom.LoginMessage(textBoxLogin.Text, passwordBoxPassword.Password), client);
                    if (cc.readMessage(client) == 1)
                    {
                        MessageBox.Show("You are logged in");
                        MainWindow mw = new MainWindow(client, cc);
                        isLogged = true;
                        textBoxLogin.Text = "";
                        passwordBoxPassword.Password = "";
                        mw.Show();
                        mw.Owner = this;
                        this.Hide();
                    }
                    else
                        MessageBox.Show("Wrong login or password");
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
