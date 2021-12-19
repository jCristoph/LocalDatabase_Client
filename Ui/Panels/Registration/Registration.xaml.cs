using System.Net.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using LocalDatabase_Client.Client;

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
            cc = new ClientConnection("127.0.0.1");
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

            string password_SHA256 = Encryption.encryption256(password);

            if (surnameTextBox.Text.Length < 2 && nameTextBox.Text.Length < 2)
            {
                mp = new MessagePanel.MessagePanel("Data is not valid", false);
                mp.ShowDialog();
                return;
            }

            cc.sendMessage(ClientCom.RegistrationMessage(surnameTextBox.Text, nameTextBox.Text, password_SHA256), sslStream); // client sends a request to login with paramteres from texbox and passwordbox
            int answer = cc.readMessage(sslStream);
            if (answer == 3)
            {
                mp = new MessagePanel.MessagePanel("Registration success", false);
            }

            else if(answer == 4)
            {
                mp = new MessagePanel.MessagePanel("User already exists", false);
            }

            else
            {
                mp = new MessagePanel.MessagePanel("Registration error", false);
            }
            mp.ShowDialog();
            this.Close();
        }

        //if user clicks back button then panel just close.
        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
