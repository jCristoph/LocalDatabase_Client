﻿using System.Net.Sockets;
using System.Windows;

namespace LocalDatabase_Client.ChangePasswordPanel
{
    public partial class ChangePasswordPanel : Window
    {
        ClientConnection cc;
        TcpClient client;
        string token;

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
                cc.readMessage(client); //checks if its done
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