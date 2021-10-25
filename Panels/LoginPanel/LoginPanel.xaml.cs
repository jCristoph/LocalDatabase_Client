﻿using System;
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
using System.Security.Cryptography;
using System.Text;

namespace LocalDatabase_Client.LoginPanel
{
    /// <summary>
    /// Logika interakcji dla klasy LoginPanel.xaml
    /// </summary>
    public partial class LoginPanel : Window
    {
        private SslStream sslStream;
        private ClientConnection cc;

        public LoginPanel()
        {
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen; //app is always in center of screen
            InitializeComponent();
        }

        //password encryption method SHA256
        public static string encryption256(string password)
        {
            Byte[] passBytes = Encoding.UTF8.GetBytes(password);
            Byte[] hashBytes = new SHA256CryptoServiceProvider().ComputeHash(passBytes);

            return BitConverter.ToString(hashBytes);
        }

        private void Connection()
        {
            cc = new ClientConnection("127.0.0.1");
            sslStream = cc.Start();
        }

        //login button event. 
        private void loginButton_Click(object sender, RoutedEventArgs e)
        {
            Task t = new Task(() => Connection()); //task that realizing a connection with server
            t.Start();
            Thread.Sleep(100); 
            if (sslStream == null) //condition if server or client is offline
            {
                MessagePanel.MessagePanel mp = new MessagePanel.MessagePanel("Error. Cannot connect with server!", false);
                mp.ShowDialog();
            }
            else
            {
                //if (client.Connected) //condition if client connects properly
                //{
                    cc.sendMessage(ClientCom.LoginMessage(textBoxLogin.Text, passwordBoxPassword.Password), sslStream); // client sends a request to login with paramteres from texbox and passwordbox
                    int answer = cc.readMessage(sslStream);
                    if (answer == 1) //condition checks if user logged properly
                    {
                        bool isPasswordChanged = false;
                        MessagePanel.MessagePanel mp = new MessagePanel.MessagePanel("Logged in!", false);
                        mp.ShowDialog();
                        if (cc.token.Equals(passwordBoxPassword.Password)) //condition checks if user changed default password
                            isPasswordChanged = true;
                        MainWindow mw = new MainWindow(sslStream, cc, isPasswordChanged);
                        textBoxLogin.Text = ""; //data form textbox is cleared to relogin
                        passwordBoxPassword.Password = ""; //data form passwordbox is cleared to relogin
                        mw.Owner = this;
                        this.Hide(); //panel is allways in background to relogin
                        mw.Show();
                    }
                    else if (answer == 2)
                    {
                        MessagePanel.MessagePanel mp = new MessagePanel.MessagePanel("You are logged in other device or your app stopped unexpectedly. Cool down and go back for 15 minutes!", false);
                        mp.ShowDialog();
                    }
                    else
                    {
                        MessagePanel.MessagePanel mp = new MessagePanel.MessagePanel("Wrong password or login.", false);
                        mp.ShowDialog();
                    }
                //}
            }
        }

        //exit button event. Close the app.
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        //help button event. Opens a help panel.
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            HelpPanel.HelpPanel hp = new HelpPanel.HelpPanel();
            hp.Show();
        }
    }
}
