using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LocalDatabase_Client
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TcpClient client;
        private ClientConnection cc;
        private bool isLogged = false;
        public MainWindow()
        {
            InitializeComponent();
            Task t = new Task(() => newThread());
            t.Start();
        }

        private void newThread()
        {
            cc = new ClientConnection(listBox, "127.0.0.1");
            client = cc.Start();
        }

        private void LoggingButton_Click(object sender, RoutedEventArgs e)
        {
            if(client.Connected)
            {
                cc.sendMessage(ClientCom.LoginMessage(textBoxLogin.Text, textBoxPassword.Text), client);
                if (cc.readMessage(client) == 1)
                {
                    cc.sendMessage(ClientCom.SendDirectoryOrderMessage(), client);
                    cc.readMessage(client);
                    MessageBox.Show("You are logged in");
                    isLogged = true;
                }
                else
                    MessageBox.Show("Wrong login or password");
            }

        }

        private void DownloadFileButton(object sender, RoutedEventArgs e)
        {
            if (client.Connected && isLogged)
            {
                cc.sendMessage(ClientCom.SendOrderMessage(((DirectoryElement)listBox.SelectedItem).path + ((DirectoryElement)listBox.SelectedItem).name), client);
                cc.downloadFile(client);
            }
            else
                MessageBox.Show("Error");
        }

        private void SendFileButton(object sender, RoutedEventArgs e)
        {
            if (client.Connected && isLogged)
            {
                cc.sendMessage(ClientCom.ReadOrderMessage(), client);
                cc.readMessage(client);
                cc.sendFile(client, @"E:\music.mp3");
            }
            else
                MessageBox.Show("Error");
        }

        private void DeleteFileButton(object sender, RoutedEventArgs e)
        {
            if (client.Connected && isLogged)
            {
                string deletedElement = ((DirectoryElement)listBox.SelectedItem).path + ((DirectoryElement)listBox.SelectedItem).name;
                cc.sendMessage(ClientCom.DeleteMessage(deletedElement), client);
                cc.readMessage(client);
            }
            else
                MessageBox.Show("Error");
        }
    }

}
