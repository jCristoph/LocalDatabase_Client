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
        private bool isMovingLeft = false;

        // 0-3 login
        // 4-5 registration
        // 6-10 main window
        // 11 change password
        // 12 folder
        // 13 send file
        List<BitmapImage> images;

        public HelpPanel()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            InitializeComponent();
            loadImages();
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
            if (!isMovingLeft)
            {
                page = 0;
                image.Source = images[page];
            }
               
        }

        private void registrationButton_Checked(object sender, RoutedEventArgs e)
        {
            if (!isMovingLeft)
            {
                page = 4;
                image.Source = images[page];
            }
        }

        //in the left of panel we have a list of subjects. This method is a event for first object of the list - main window. sets right page of subject list and changes the source image to right one for the subject.
        private void main_windowButton_Checked(object sender, RoutedEventArgs e)
        {
            if (!isMovingLeft)
            {
                page = 6;
                image.Source = images[page];
            }
            isMovingLeft = false;
        }

        //in the left of panel we have a list of subjects. This method is a event for first object of the list - changing password. sets right page of subject list and changes the source image to right one for the subject.
        private void change_passwordButton_Checked(object sender, RoutedEventArgs e)
        {
            
            page = 11;
            image.Source = images[page];
        }

        //in the left of panel we have a list of subjects. This method is a event for first object of the list - creating new folder. sets right page of subject list and changes the source image to right one for the subject.
        private void creating_foldersButton_Checked(object sender, RoutedEventArgs e)
        {
            
            page = 12;
            image.Source = images[page];
        }

        //in the left of panel we have a list of subjects. This method is a event for first object of the list - sending. sets right page of subject list and changes the source image to right one for the subject.
        private void send_fileButton_Checked(object sender, RoutedEventArgs e)
        {
            
            page = 13;
            image.Source = images[page];
        }

        //first of two navigation buttons - it returns in list 
        private void leftButton_Click(object sender, RoutedEventArgs e)
        {
            isMovingLeft = true;
            if (page > 0)
            {
                page--;
                
                if (page == 3)
                {
                    loginButton.IsChecked = true;
                }

                if (page == 5)
                {
                    registrationButton.IsChecked = true;
                }

                if (page == 10)
                {
                    main_windowButton.IsChecked = true;
                }

                if (page == 11)
                {
                    change_passwordButton.IsChecked = true;
                }

                if (page == 12)
                {
                    creating_foldersButton.IsChecked = true;
                }

            }
            image.Source = images[page];
        }

        //second of two navigation buttons - it goes forward in list
        private void rightButton_Click(object sender, RoutedEventArgs e)
        {
            isMovingLeft = false;
            if (page < images.Count - 1)
            {
                page++;

                if(page == 0)
                {
                    loginButton.IsChecked = true;
                }

                if (page == 4)
                {
                    registrationButton.IsChecked = true;
                }

                if (page == 6)
                {
                    main_windowButton.IsChecked = true;
                }

                if (page == 11)
                {
                    change_passwordButton.IsChecked = true;
                }

                if (page == 12)
                {
                    creating_foldersButton.IsChecked = true;
                }

                if (page == 13)
                {
                    send_fileButton.IsChecked = true;
                }

                image.Source = images[page];
            }
        }
  

        private void loadImages()
        {
            images = new List<BitmapImage>();
            images.Add(new BitmapImage(new Uri("/Ui/Images/HelpImages/login.png", UriKind.Relative))); 
            images.Add(new BitmapImage(new Uri("/Ui/Images/HelpImages/login_ok.png", UriKind.Relative))); 
            images.Add(new BitmapImage(new Uri("/Ui/Images/HelpImages/login_bad.png", UriKind.Relative)));
            images.Add(new BitmapImage(new Uri("/Ui/Images/HelpImages/login_error.png", UriKind.Relative))); 
          
            images.Add(new BitmapImage(new Uri("/Ui/Images/HelpImages/registration.png", UriKind.Relative)));
            images.Add(new BitmapImage(new Uri("/Ui/Images/HelpImages/registration_success.png", UriKind.Relative)));

            images.Add(new BitmapImage(new Uri("/Ui/Images/HelpImages/main_window_1.png", UriKind.Relative))); 
            images.Add(new BitmapImage(new Uri("/Ui/Images/HelpImages/main_window_2.png", UriKind.Relative))); 
            images.Add(new BitmapImage(new Uri("/Ui/Images/HelpImages/main_window_3.png", UriKind.Relative))); 
            images.Add(new BitmapImage(new Uri("/Ui/Images/HelpImages/main_window_4.png", UriKind.Relative))); 
            images.Add(new BitmapImage(new Uri("/Ui/Images/HelpImages/main_window_5.png", UriKind.Relative))); 

            images.Add(new BitmapImage(new Uri("/Ui/Images/HelpImages/change_password.png", UriKind.Relative))); 

            images.Add(new BitmapImage(new Uri("/Ui/Images/HelpImages/new_folder.png", UriKind.Relative)));

            images.Add(new BitmapImage(new Uri("/Ui/Images/HelpImages/send_file.png", UriKind.Relative))); 
        }

        
    }
}
