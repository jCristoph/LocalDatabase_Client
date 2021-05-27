using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace LocalDatabase_Client.SharePanel
{
    /// <summary>
    /// Logika interakcji dla klasy ShareFilePanel.xaml
    /// </summary>
    public partial class ShareFilePanel : Window
    {
        string senderToken;
        string path;
        TcpClient client;
        ClientConnection cc;
        ObservableCollection<User> users;
        ObservableCollection<User> sharingUsers;
        public ShareFilePanel(ClientConnection cc, TcpClient client, string token, string path)
        {
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            InitializeComponent();
            senderToken = token;
            this.client = client;
            this.cc = cc;
            this.path = path;
            cc.sendMessage(ClientCom.SendUsersOrderMessage(token), client);
            users = cc.readUsers(client);
            cc.sendMessage(ClientCom.UsersSharingMessage(path), client);
            sharingUsers = cc.readUsers(client);
            listView.ItemsSource = sharingUsers;
            comboBox.ItemsSource = users;
        }

        private void ShareButton(object sender, RoutedEventArgs e)
        {
            var userToShare = comboBox.SelectedItem as User;
            cc.sendMessage(ClientCom.ShareMessage(senderToken, userToShare.token, path, "1"), client);
        }

        private void BackButton(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
