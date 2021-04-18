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
        private bool isLogged;
        public MainWindow()
        {
            InitializeComponent();
            Task t = new Task(() => newThread());
            t.Start();
        }

        private void newThread()
        {
            cc = new ClientConnection(text, "127.0.0.1");
            client = cc.Start();
        }

        private void LoggingButton_Click(object sender, RoutedEventArgs e)
        {
            if (client.Connected)
                {
                    cc.sendMessage(ClientCom.LoginMessage(textBoxLogin.Text, textBoxPassword.Text), client);
                    cc.readMessage(client);
                    cc.sendMessage(ClientCom.SendDirectoryOrderMessage(), client);
                    cc.readMessage(client);
                }
        }

        private void DownloadFileButton(object sender, RoutedEventArgs e)
        {
            
            try
            {
                if (client.Connected)
                {
                    cc.sendMessage(ClientCom.SendOrderMessage(@"C:\Directory_test\plik1.txt"), client);
                    progressBar.IsIndeterminate = true;
                    progressBar.Visibility = Visibility.Visible;
                    cc.downloadFile(client);
                  
                }
            }
            catch (Exception exception)
            {
                progressBar.IsIndeterminate = false;
                progressBar.Visibility = Visibility.Hidden;
                Console.WriteLine(exception.Message);
            }
        }

        private void SendFileButton(object sender, RoutedEventArgs e)
        {
            try
            {
                if (client.Connected)
                {
                    cc.sendMessage(ClientCom.ReadOrderMessage(), client);
                    cc.readMessage(client);
                    progressBar.IsIndeterminate = true;
                    progressBar.Visibility = Visibility.Visible;
                    cc.sendFile(client, @"E:\music.mp3");
                }
            }
            catch (Exception exception)
            {
                progressBar.IsIndeterminate = false;
                progressBar.Visibility = Visibility.Hidden;
                Console.WriteLine(exception.Message);
            }

        }
    }

}
