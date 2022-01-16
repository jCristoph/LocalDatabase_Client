using System;
using System.Net.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace LocalDatabase_Client.Registration
{
    /// <summary>
    /// Logika interakcji dla klasy Registration.xaml
    /// </summary>
    public partial class Registration : Window
    {
        private SslStream sslStream;
        private ClientConnection cc;
        public Registration()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen; //app is always in center of screen
            InitializeComponent();
        }

        private void Connection()
        {
            cc = new ClientConnection();
            cc.Start();
            sslStream = cc.SslStream;
        }

        //click button event. If the name and surname are longer than 2 system add new user - > look at Database/DatabaseManager.cs => AddUser()
        //if the condition is not met then user has to try again.
        private void createButton_Click(object sender, RoutedEventArgs e)
        {

            MessagePanel.MessagePanel mp;

            Task t = new Task(() => Connection()); //task that realizing a connection with server
            t.Start();
            Thread.Sleep(100);
            if (sslStream == null) //condition if server or client is offline
            {
              
                mp = new MessagePanel.MessagePanel("Error. Cannot connect with server!", false);
               mp.ShowDialog();
                return;
            }

            string password = "0";

            if (passwordBoxPassword1.Password == passwordBoxPassword2.Password)
                password = passwordBoxPassword1.Password;
            else
                mp = new MessagePanel.MessagePanel("Passwords are different!", false);

            string password_SHA256 = Security.PasswordEncryption.encryption256(password);
            if (surnameTextBox.Text.Length > 2 && nameTextBox.Text.Length > 2)
            {
                cc.sendMessage(ClientCom.RegistrationMessage(surnameTextBox.Text, nameTextBox.Text, password_SHA256), sslStream); // client sends a request to login with paramteres from texbox and passwordbox
                dynamic answer = cc.readMessage(sslStream);
                Console.WriteLine(answer.GetType());
                if (answer.GetType().ToString().Equals("System.String"))
                {
                    string token = (((string)answer).Replace("<Task=Response><Content>", "")).Replace("</Content></Task><EOM>", "");
                    mp = new MessagePanel.MessagePanel("Registration success.\nYour login is " + nameTextBox.Text + "." + surnameTextBox.Text, false);
                    string key = Security.KeyGenerator.Generate();
                    Security.KeyHandling.SaveKey(token, key);
                }
                else
                {
                    mp = new MessagePanel.MessagePanel("Registration error", false);
                }
                mp.ShowDialog();
                this.Close();
            }
            else
            {
                mp = new MessagePanel.MessagePanel("Data is not valid", false);
                mp.ShowDialog();
            }

        }

    //if user clicks back button then panel just close.
    private void backButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}