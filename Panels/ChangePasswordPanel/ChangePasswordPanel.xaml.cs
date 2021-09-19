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
    public partial class ChangePasswordPanel : Window
    {
        ClientConnection cc;
        TcpClient client;
        string token;
        public bool isDone;

        //constructor
        public ChangePasswordPanel(ClientConnection cc, TcpClient client, string token)
        {
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen; //app is always in center of screen
            InitializeComponent();
            this.cc = cc;
            this.client = client;
            this.token = token;
        }

        //event for change password button. 
        private void changeButton_Click(object sender, RoutedEventArgs e)
        {
            if(passwordBox.Password.Equals(passwordBox2.Password)) //condition if new password is the same in both of password boxes.
            {
                cc.sendMessage(ClientCom.ChangePasswordMessage(passwordBox.Password, token), client); //send message with request to set new password in db
                if (cc.readMessage(client) == 404)
                    isDone = false;
                else
                    isDone = true;
                this.Close();
            }
        }

        //back button event. close the panel
        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
