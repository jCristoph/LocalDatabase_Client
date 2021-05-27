﻿using System;
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

namespace LocalDatabase_Client.MessagePanel
{
    /// <summary>
    /// Logika interakcji dla klasy MessagePanel.xaml
    /// </summary>
    public partial class MessagePanel : Window
    {
        public bool answear = true;
        public MessagePanel(string content, bool yesNoOK)
        {
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            InitializeComponent();
            message.Text = content;
            if(!yesNoOK)
            {
                yesButton.Visibility = Visibility.Hidden;
                yesButton.Visibility = Visibility.Collapsed;
                okNoButton.Content = "OK";
            }
            DataContext = this;
        }

        private void yesButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void okNoButton_Click(object sender, RoutedEventArgs e)
        {
            answear = false;
            this.Close();
        }
    }
}
