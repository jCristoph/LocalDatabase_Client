using System;
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

namespace LocalDatabase_Client.HelpPanel
{
    /// <summary>
    /// Logika interakcji dla klasy HelpPanel.xaml
    /// </summary>
    public partial class HelpPanel : Window
    {
        private int page;
        List<BitmapImage> images;

        public HelpPanel()
        {
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            InitializeComponent();
            loadImages();
            image.Source =images[0];
            page = 0;
            loginButton.IsChecked = true;
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void loginButton_Checked(object sender, RoutedEventArgs e)
        {
            page = 0;
            image.Source = images[page];
        }

        private void main_windowButton_Checked(object sender, RoutedEventArgs e)
        {
            page = 4;
            image.Source = images[page];
        }

        private void change_passwordButton_Checked(object sender, RoutedEventArgs e)
        {
            page = 10;
            image.Source = images[page];
        }

        private void creating_foldersButton_Checked(object sender, RoutedEventArgs e)
        {
            page = 11;
            image.Source = images[page];
        }

        private void sharingButton_Checked(object sender, RoutedEventArgs e)
        {
            page = 13;
            image.Source = images[page];
        }

        private void leftButton_Click(object sender, RoutedEventArgs e)
        {
            if(page > 0)
            {
                page--;
                switch (page)
                {
                    case 3:
                        loginButton.IsChecked = true;
                        break;
                    case 9:
                        main_windowButton.IsChecked = true;
                        break;
                    case 10:
                        change_passwordButton.IsChecked = true;
                        break;
                    case 11:
                        creating_foldersButton.IsChecked = true;
                        break;
                    case 12:
                        send_fileButton.IsChecked = true;
                        break;
                    case 13:
                        sharingButton.IsChecked = true;
                        break;
                    default:
                        image.Source = images[page];
                        break;
                }
            }
        }

        private void rightButton_Click(object sender, RoutedEventArgs e)
        {
            if(page < 13)
            {
                page++;
                switch (page)
                {
                    case 0:
                        loginButton.IsChecked = true;
                        break;
                    case 4:
                        main_windowButton.IsChecked = true;
                        break;
                    case 10:
                        change_passwordButton.IsChecked = true;
                        break;
                    case 11:
                        creating_foldersButton.IsChecked = true;
                        break;
                    case 12:
                        send_fileButton.IsChecked = true;
                        break;
                    case 13:
                        sharingButton.IsChecked = true;
                        break;
                    default:
                        image.Source = images[page];
                        break;
                }
            }

        }
        private void loadImages()
        {
            images = new List<BitmapImage>();
            images.Add(new BitmapImage(new Uri("/Images/HelpImages/login.png", UriKind.Relative))); // page 0
            images.Add(new BitmapImage(new Uri("/Images/HelpImages/login_ok.png", UriKind.Relative))); // page 1
            images.Add(new BitmapImage(new Uri("/Images/HelpImages/login_bad.png", UriKind.Relative))); // page 2
            images.Add(new BitmapImage(new Uri("/Images/HelpImages/login_error.png", UriKind.Relative))); // page 3
            images.Add(new BitmapImage(new Uri("/Images/HelpImages/main_window_1.png", UriKind.Relative))); // page 4
            images.Add(new BitmapImage(new Uri("/Images/HelpImages/main_window_2.png", UriKind.Relative))); // page 5
            images.Add(new BitmapImage(new Uri("/Images/HelpImages/main_window_3.png", UriKind.Relative))); // page 6
            images.Add(new BitmapImage(new Uri("/Images/HelpImages/main_window_4.png", UriKind.Relative))); // page 7
            images.Add(new BitmapImage(new Uri("/Images/HelpImages/main_window_5.png", UriKind.Relative))); // page 8
            images.Add(new BitmapImage(new Uri("/Images/HelpImages/main_window_6.png", UriKind.Relative))); // page 9
            images.Add(new BitmapImage(new Uri("/Images/HelpImages/change_password.png", UriKind.Relative))); // page 10
            images.Add(new BitmapImage(new Uri("/Images/HelpImages/new_folder.png", UriKind.Relative))); // page 11
            images.Add(new BitmapImage(new Uri("/Images/HelpImages/send_file.png", UriKind.Relative))); // page 12
            images.Add(new BitmapImage(new Uri("/Images/HelpImages/share.png", UriKind.Relative))); // page 13
        }

        private void send_fileButton_Checked(object sender, RoutedEventArgs e)
        {
            page = 12;
            image.Source = images[page];
        }
    }
}
