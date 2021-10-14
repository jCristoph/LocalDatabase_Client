using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;


namespace LocalDatabase_Client
{
    public partial class MainWindow : Window
    {
        private SslStream sslStream;
        private ClientConnection cc;
        private DirectoryManager directoryManager;
        private double limit;
        private ObservableCollection<DirectoryElement> currentDirectory;
        private DirectoryElement currentFolder;
        private string token;
        
        public MainWindow(SslStream sslStream, ClientConnection cc, bool isPasswordChanged)
        {
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen; //app is visible in center
            InitializeComponent();
            if (isPasswordChanged) //condition if password was changed from first (when account is created the password is the same as token so user should change it)
            {
                MessagePanel.MessagePanel mp = new MessagePanel.MessagePanel("Zmień hasło", false);
                mp.Show();
            }

            progressBar.Maximum = 100;
            progressBar.Minimum = 0;

            token = cc.token;
            this.sslStream = sslStream;
            this.cc = cc;
            this.limit = cc.limit;
            directoryManager = new DirectoryManager();
            currentDirectory = new ObservableCollection<DirectoryElement>();
            currentFolder = new DirectoryElement("\\Main_Folder", 0, "None", true);
            currentFolderTextBlock.Text = currentFolder.path + currentFolder.name;
            listView.ItemsSource = currentDirectory;
            DataContext = this;
            refreshList();
        }

        /// <summary>
        /// method that refreshes a container of files
        /// </summary>
        private void refreshList()
        {
            if (sslStream != null)
            {
                do
                {
                    cc.sendMessage(ClientCom.SendDirectoryOrderMessage(token), sslStream); //send request to server to send list of directory
                    string data = null;
                    try
                    {
                        data = cc.readMessage(sslStream);
                        directoryManager.directoryElements.Clear();
                        ClientCom.SendDirectoryRecognizer(data, directoryManager);
                        Application.Current.Dispatcher.Invoke(new Action(() => { currentDirectory.Clear(); })); //special line for changing data of other thread
                        foreach (var a in directoryManager.directoryElements)
                        {
                            if (a.pathArray[a.pathArray.Count - 1] == currentFolder.name)
                                Application.Current.Dispatcher.Invoke(new Action(() => { currentDirectory.Add(a); }));
                        }
                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            refreshTextBlock.Text = "Ostatnie odświeżenie: " + DateTime.Now;
                            sizeTextBlock.Text = "Zużyto " + Math.Round(directoryManager.usedSpace(), 2) + "GB / " + limit + "GB";
                        }));
                    }
                    catch (Exception e)
                    {
                        
                        var mp = new MessagePanel.MessagePanel("Utracono połączenie z serwerem, zaloguj się jeszcze raz", false);
                        mp.ShowDialog();
                        Owner.Show();
                        this.Close();
                    }
                } while (directoryManager.directoryElements == null);
            }
        }

        //an event for download (if object is a file) or open (if object is a folder) button
        private void DownloadOrOpenButton(object sender, RoutedEventArgs e)
        {
            Button btn = ((Button)sender);
            if (btn.Content.Equals("Pobierz"))
            {
                try
                {
                    //client sends a message with order to download a file. From button (cast) we know what file should be downloaded.
                    cc.sendMessage(ClientCom.SendOrderMessage((((DirectoryElement)btn.DataContext).path).Replace("Main_Folder", "Main_Folder\\" + token) + ((DirectoryElement)btn.DataContext).name, token), sslStream);
                    int answer = cc.readMessage(sslStream);
                    if (answer == -1)
                    {
                        var mp = new MessagePanel.MessagePanel("Utracono połączenie z serwerem, zaloguj się jeszcze raz", false);
                        mp.ShowDialog();
                        Owner.Show();
                        this.Close();
                    }
                    else if (answer == 404)
                    {
                        Owner.Show();
                        MessagePanel.MessagePanel mp1 = new MessagePanel.MessagePanel("Sesja wygasła. Zaloguj się ponownie", false);
                        mp1.ShowDialog();
                        this.Close();
                    }
                    else
                    {
                        var fileTransporter = new FileTransporter("127.0.0.1", ((DirectoryElement)btn.DataContext).name, ((DirectoryElement)btn.DataContext).size, progressBar);
                        fileTransporter.connectAsClient();
                        fileTransporter.recieveFile(refreshList);
                    }

                }
                catch
                {
                    var mp = new MessagePanel.MessagePanel("Utracono połączenie z serwerem, zaloguj się jeszcze raz", false);
                    mp.ShowDialog();
                    Owner.Show();
                    this.Close();
                }
            }
            else if (btn.Content.Equals("Otwórz")) //condition if the object isnt a file - is a folder - we cant download a folder - only one file in the same time. Then the button is a open button which opens a subfolder and list it
            {
                currentFolder = ((DirectoryElement)btn.DataContext);
                currentDirectory.Clear();
                foreach (var a in directoryManager.directoryElements)
                {
                    if (a.pathArray[a.pathArray.Count - 1] == currentFolder.name)
                        currentDirectory.Add(a);
                }
                currentFolderTextBlock.Text = currentFolder.path + currentFolder.name;
            }
            else
            {
                MessagePanel.MessagePanel mp = new MessagePanel.MessagePanel("Błąd", false);
                mp.ShowDialog();
            }
           
        }

        //event for send file button
        private void SendFileButton(object sender, RoutedEventArgs e)
        {
           //if (sslStream.Connected)
           //{
                Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog(); //special Windows panel for file browsing
                Nullable<bool> result = dlg.ShowDialog();
                string filename = dlg.FileName;
                if(!filename.Equals(""))
                {
                if (currentDirectory.Any(x => x.name == dlg.SafeFileName)) //condition if file could be overwrite
                {
                    MessagePanel.MessagePanel mp = new MessagePanel.MessagePanel("Czy na pewno chcesz nadpisać plik?", true);
                    mp.ShowDialog();
                    if (mp.answear.Equals(true))
                    {
                        if (result == true)
                        {
                            cc.sendMessage(ClientCom.ReadOrderMessage(currentFolder, token, dlg.SafeFileName), sslStream);
                            int answer = cc.readMessage(sslStream);
                            if (answer == -1)
                            {
                                mp = new MessagePanel.MessagePanel("Utracono połączenie z serwerem, zaloguj się jeszcze raz", false);
                                mp.ShowDialog();
                                Owner.Show();
                                this.Close();
                            }
                            else if (answer == 404)
                            {
                                Owner.Show();
                                MessagePanel.MessagePanel mp1 = new MessagePanel.MessagePanel("Sesja wygasła. Zaloguj się ponownie", false);
                                mp1.ShowDialog();
                                this.Close();
                            }
                            else
                            {
                                var fileTransporter = new FileTransporter("127.0.0.1", filename, new FileInfo(dlg.FileName).Length, progressBar);
                                fileTransporter.connectAsClient();
                                fileTransporter.sendFile(refreshList);
                            }
                        }
                        else
                        {
                            mp = new MessagePanel.MessagePanel("Błąd", false);
                            mp.Show();
                        }
                    }
                }
                else if (result == true)
                {
                    cc.sendMessage(ClientCom.ReadOrderMessage(currentFolder, token, dlg.SafeFileName), sslStream); //client sends request for server to read file
                    int answer = cc.readMessage(sslStream);
                    if (answer == -1)
                    {
                        var mp = new MessagePanel.MessagePanel("Utracono połączenie z serwerem, zaloguj się jeszcze raz", false);
                        mp.ShowDialog();
                        Owner.Show();
                        this.Close();
                    }
                    else if (answer == 404) //client waits for answer
                    {
                        Owner.Show();
                        MessagePanel.MessagePanel mp1 = new MessagePanel.MessagePanel("Sesja wygasła. Zaloguj się ponownie", false);
                        mp1.ShowDialog();
                        this.Close();
                    }
                    else
                    {
                        var fileTransporter = new FileTransporter("127.0.0.1", filename, new FileInfo(dlg.FileName).Length, progressBar);
                        fileTransporter.connectAsClient();
                        fileTransporter.sendFile(refreshList);
                    }
                }
                else
                {
                    MessagePanel.MessagePanel mp = new MessagePanel.MessagePanel("Błąd", false);
                    mp.Show();
                }
            }
            //}
        }

        //delete button event
        private void DeleteFileButton(object sender, RoutedEventArgs e)
        {
            MessagePanel.MessagePanel mp = new MessagePanel.MessagePanel("Czy jesteś pewien, że chcesz usunąć ten element?", true); //ask user if he's sure to delete
            mp.ShowDialog();
            Button btn = ((Button)sender);
            try
            {
                DirectoryElement temp = (DirectoryElement)btn.DataContext;
                string deletedElement = temp.path.Replace("Main_Folder", "Main_Folder\\" + token) + temp.name;
                cc.sendMessage(ClientCom.DeleteMessage(deletedElement, ((DirectoryElement)btn.DataContext).isFolder, token), sslStream); //client sends request to delete file or folder
                int answer = cc.readMessage(sslStream);
                if (answer == -1)
                {
                    mp = new MessagePanel.MessagePanel("Utracono połączenie z serwerem, zaloguj się jeszcze raz", false);
                    mp.ShowDialog();
                    Owner.Show();
                    this.Close();
                }
                else if (answer == 404) //client waits for answer
                {
                    Owner.Show();
                    MessagePanel.MessagePanel mp1 = new MessagePanel.MessagePanel("Sesja wygasła. Zaloguj się ponownie", false);
                    mp1.ShowDialog();
                    this.Close();
                }
                else
                {
                    refreshList();
                    // TODO: client get a answer if file or folder was deleted
                }
            }
            catch (Exception ex)
            {
                mp = new MessagePanel.MessagePanel("Coś poszło nie tak, spróbuj jeszcze raz.", false);
                mp.Show();
            }
        }

        //event for return button. It returns in directory paths. Change actual current folder.
        private void ReturnButton(object sender, RoutedEventArgs e)
        {
            if (!currentFolder.name.Equals("Main_Folder"))
            {
                currentFolder = directoryManager.directoryElements.First(x => x.name.Equals(currentFolder.pathArray[currentFolder.pathArray.Count - 1]));
                currentDirectory.Clear();
                foreach (var a in directoryManager.directoryElements)
                {
                    if (a.pathArray[a.pathArray.Count - 1] == currentFolder.name)
                        currentDirectory.Add(a);
                }
            }
            else
            {
                MessagePanel.MessagePanel mp = new MessagePanel.MessagePanel("Jesteś w głównym folderze", false);
                mp.ShowDialog();
            }
            currentFolderTextBlock.Text = currentFolder.path + currentFolder.name;
        }

        //log out button event. Right now its just close the window and shows a login window but in the future it will be send a request to logout also in server side.
        private void LogOutButton(object sender, RoutedEventArgs e)
        {
            cc.sendMessage(ClientCom.LogoutMessage(token), sslStream);
            Owner.Show();
            this.Close();
        }

        //create folder button event
        private void CreateFolderButton(object sender, RoutedEventArgs e)
        {
            CreateFolderPanel.CreateFolderPanel cfp = new CreateFolderPanel.CreateFolderPanel(); //there shows new panel where user has to enter the name of new folder.
            cfp.ShowDialog();
            if(cfp.folderName != null) //condition if the new folder name isnt empty
            {
                cc.sendMessage(ClientCom.CreateFolderMessage(currentFolder, token, cfp.folderName), sslStream); //client sends request to server to create new folder with name of folderName parameter
                int answer = cc.readMessage(sslStream);
                if (answer == -1)
                {
                    var mp = new MessagePanel.MessagePanel("Utracono połączenie z serwerem, zaloguj się jeszcze raz", false);
                    mp.ShowDialog();
                    Owner.Show();
                    this.Close();
                }
                else if (answer == 404)
                {
                    Owner.Show();
                    MessagePanel.MessagePanel mp1 = new MessagePanel.MessagePanel("Sesja wygasła. Zaloguj się ponownie", false);
                    mp1.ShowDialog();
                    this.Close();
                }
                else
                {
                    refreshList();
                }
                    // TODO: client checks if its done
            }
        }

        //exit button event. Right now its just close the app but in the future it will be send a request to logout also in server side.
        private void ExitButton(object sender, RoutedEventArgs e)
        {
            cc.sendMessage(ClientCom.LogoutMessage(token), sslStream);
            Owner.Close();
            this.Close();
        }

        //change password button event. Shows panel where user can change the password
        private void ChangePasswordButton(object sender, RoutedEventArgs e)
        {
            ChangePasswordPanel.ChangePasswordPanel chp = new ChangePasswordPanel.ChangePasswordPanel(cc, sslStream, token);
            chp.ShowDialog();
            if(!chp.isDone)
            {
                Owner.Show();
                MessagePanel.MessagePanel mp1 = new MessagePanel.MessagePanel("Sesja wygasła. Zaloguj się ponownie", false);
                mp1.ShowDialog();
                this.Close();
            }
        }

        //help button event. Shows help panel
        private void HelpButton(object sender, RoutedEventArgs e)
        {
            HelpPanel.HelpPanel hp = new HelpPanel.HelpPanel();
            hp.Show();
        }
    }

}
