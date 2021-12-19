using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Imaging;

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
            WindowStartupLocation = WindowStartupLocation.CenterScreen; //app is always in center of screen
            InitializeComponent();
            loadImages(); //loads images with tips
            image.Source = images[0]; //set source for gui
            page = 0;
            loginButton.IsChecked = true;
        }

        //back button event. Close the panel.
        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        //in the left of panel we have a list of subjects. This method is a event for first object of the list - logging. sets right page of subject list and changes the source image to right one for the subject.
        private void loginButton_Checked(object sender, RoutedEventArgs e)
        {
            page = 0;
            image.Source = images[page];
        }

        //in the left of panel we have a list of subjects. This method is a event for first object of the list - main window. sets right page of subject list and changes the source image to right one for the subject.
        private void main_windowButton_Checked(object sender, RoutedEventArgs e)
        {
            page = 4;
            image.Source = images[page];
        }

        //in the left of panel we have a list of subjects. This method is a event for first object of the list - changing password. sets right page of subject list and changes the source image to right one for the subject.
        private void change_passwordButton_Checked(object sender, RoutedEventArgs e)
        {
            page = 10;
            image.Source = images[page];
        }

        //in the left of panel we have a list of subjects. This method is a event for first object of the list - creating new folder. sets right page of subject list and changes the source image to right one for the subject.
        private void creating_foldersButton_Checked(object sender, RoutedEventArgs e)
        {
            page = 11;
            image.Source = images[page];
        }

        //in the left of panel we have a list of subjects. This method is a event for first object of the list - sending. sets right page of subject list and changes the source image to right one for the subject.
        private void send_fileButton_Checked(object sender, RoutedEventArgs e)
        {
            page = 12;
            image.Source = images[page];
        }

        //first of two navigation buttons - it returns in list 
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
                    default:
                        image.Source = images[page];
                        break;
                }
            }
        }

        //second of two navigation buttons - it goes forward in list
        private void rightButton_Click(object sender, RoutedEventArgs e)
        {
            if(page < 12)
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
                    default:
                        image.Source = images[page];
                        break;
                }
            }

        }

        //method loads every images to one list. All of them are clearly named
        private void loadImages()
        {
            images = new List<BitmapImage>();
            images.Add(new BitmapImage(new Uri("Ui/Images/HelpImages/login.png", UriKind.Relative))); // page 0
            images.Add(new BitmapImage(new Uri("Ui/Images/HelpImages/login_ok.png", UriKind.Relative))); // page 1
            images.Add(new BitmapImage(new Uri("Ui/Images/HelpImages/login_bad.png", UriKind.Relative))); // page 2
            images.Add(new BitmapImage(new Uri("Ui/Images/HelpImages/login_error.png", UriKind.Relative))); // page 3
            images.Add(new BitmapImage(new Uri("Ui/Images/HelpImages/main_window_1.png", UriKind.Relative))); // page 4
            images.Add(new BitmapImage(new Uri("Ui/Images/HelpImages/main_window_2.png", UriKind.Relative))); // page 5
            images.Add(new BitmapImage(new Uri("Ui/Images/HelpImages/main_window_3.png", UriKind.Relative))); // page 6
            images.Add(new BitmapImage(new Uri("Ui/Images/HelpImages/main_window_4.png", UriKind.Relative))); // page 7
            images.Add(new BitmapImage(new Uri("Ui/Images/HelpImages/main_window_5.png", UriKind.Relative))); // page 8
            images.Add(new BitmapImage(new Uri("Ui/Images/HelpImages/main_window_6.png", UriKind.Relative))); // page 9
            images.Add(new BitmapImage(new Uri("Ui/Images/HelpImages/change_password.png", UriKind.Relative))); // page 10
            images.Add(new BitmapImage(new Uri("Ui/Images/HelpImages/new_folder.png", UriKind.Relative))); // page 11
            images.Add(new BitmapImage(new Uri("Ui/Images/HelpImages/send_file.png", UriKind.Relative))); // page 12
        }
    }
}
