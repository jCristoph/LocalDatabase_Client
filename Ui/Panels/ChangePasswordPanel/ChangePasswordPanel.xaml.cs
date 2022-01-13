using System.Net.Security;
using System.Windows;

namespace LocalDatabase_Client.ChangePasswordPanel
{
    public partial class ChangePasswordPanel : Window
    {
        ClientConnection cc;
        SslStream sslStream;
        string token;
        public bool isDone;

        //constructor
        public ChangePasswordPanel(ClientConnection cc, SslStream sslStream, string token)
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen; //app is always in center of screen
            InitializeComponent();
            this.cc = cc;
            this.sslStream = sslStream;
            this.token = token;
        }

        //event for change password button. 
        private void changeButton_Click(object sender, RoutedEventArgs e)
        {
            if(passwordBox.Password.Equals(passwordBox2.Password)) //condition if new password is the same in both of password boxes.
            {
                cc.sendMessage(ClientCom.ChangePasswordMessage(Security.PasswordEncryption.encryption256(passwordBox.Password), token), sslStream); //send message with request to set new password in db
                if (cc.readMessage(sslStream) == 404)
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
