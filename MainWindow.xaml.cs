using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;


namespace LocalDatabase_Client
{
    public partial class MainWindow : Window
    {
        private TcpClient client;
        private ClientConnection cc;
        private DirectoryManager directoryManager;
        private double limit;
        private ObservableCollection<DirectoryElement> currentDirectory;
        private DirectoryElement currentFolder;
        private string token;
        
        public MainWindow(TcpClient client, ClientConnection cc, bool isPasswordChanged)
        {
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen; //app is visible in center
            InitializeComponent();
            if (isPasswordChanged) //condition if password was changed from first (when account is created the password is the same as token so user should change it)
            {
                MessagePanel.MessagePanel mp = new MessagePanel.MessagePanel("Zmień hasło", false);
                mp.Show();
            }
            token = cc.token;
            this.client = client;
            this.cc = cc;
            this.limit = cc.limit;
            directoryManager = new DirectoryManager();
            currentDirectory = new ObservableCollection<DirectoryElement>();
            currentFolder = new DirectoryElement("\\Main_Folder", 0, "None", true);
            currentFolderTextBlock.Text = currentFolder.path + currentFolder.name;
            listView.ItemsSource = currentDirectory;
            DataContext = this;
            refreshingList();
        }

        /// <summary>
        /// TODO
        /// refreshing method. Works in other thread (asynchronous) but we have to do it synchronous.
        /// </summary>
        private void refreshingList()
        {
            if (client != null)
            {
                if (!cc.isBusy)
                {
                DirectoryRequest: //a label is used istead of loop - i use it when the data from server are broken
                    cc.sendMessage(ClientCom.SendDirectoryOrderMessage(token), client); //send request to server to send list of directory
                    cc.getDirectory(client, directoryManager); //special method for reading directory
                    if (directoryManager.directoryElements != null)
                    {
                        Application.Current.Dispatcher.Invoke(new Action(() => { currentDirectory.Clear(); })); //special line for changing data of other thread
                        foreach (var a in directoryManager.directoryElements)
                        {
                            if (a.pathArray[a.pathArray.Count - 1] == currentFolder.name)
                                Application.Current.Dispatcher.Invoke(new Action(() => { currentDirectory.Add(a); }));
                        }
                        Application.Current.Dispatcher.Invoke(new Action(() => {
                            refreshTextBlock.Text = "Ostatnie odświeżenie: " + DateTime.Now;
                            sizeTextBlock.Text = "Zużyto " + Math.Round(directoryManager.usedSpace(), 2) + "GB / " + limit + "GB";
                        }));
                    }
                    else
                    {
                        goto DirectoryRequest;
                    }
                }
            }
        }

        //an event for download (if object is a file) or open (if object is a folder) button
        private void DownloadOrOpenButton(object sender, RoutedEventArgs e)
        {
            Button btn = ((Button)sender);
            if (btn.Content.Equals("Pobierz"))
            {
                if (client.Connected)
                {
                    try
                    {
                        //client sends a message with order to download a file. From button (cast) we know what file should be downloaded.
                        cc.sendMessage(ClientCom.SendOrderMessage((((DirectoryElement)btn.DataContext).path).Replace("Main_Folder", "Main_Folder\\" + token) + ((DirectoryElement)btn.DataContext).name, token), client);
                        if (cc.readMessage(client) == 404)
                        {
                            Owner.Show();
                            MessagePanel.MessagePanel mp1 = new MessagePanel.MessagePanel("Sesja wygasła. Zaloguj się ponownie", false);
                            mp1.ShowDialog();
                            this.Close();
                        }
                        else
                            cc.downloadFile(client);
                    }
                    catch
                    {
                        MessagePanel.MessagePanel mp = new MessagePanel.MessagePanel("Wybierz plik do pobrania", false);
                        mp.ShowDialog();
                    }
                }
                else
                {
                    MessagePanel.MessagePanel mp = new MessagePanel.MessagePanel("Błąd", false);
                    mp.ShowDialog();
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
           if (client.Connected)
           {
                Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog(); //special Windows panel for file browsing
                Nullable<bool> result = dlg.ShowDialog();
                string filename = dlg.FileName;
                if(!filename.Equals(""))
                {
                    if (new FileInfo(filename).Length < 1100000000) //condition for 1gb limit file
                    {
                        if (currentDirectory.Any(x => x.name == dlg.SafeFileName)) //condition if file could be overwrite
                        {
                            MessagePanel.MessagePanel mp = new MessagePanel.MessagePanel("Czy na pewno chcesz nadpisać plik?", true);
                            mp.ShowDialog();
                            if (mp.answear.Equals(true))
                            {
                                if (result == true)
                                {
                                    cc.sendMessage(ClientCom.ReadOrderMessage(currentFolder, token, dlg.SafeFileName), client);
                                    if (cc.readMessage(client) == 404)
                                    {
                                        Owner.Show();
                                        MessagePanel.MessagePanel mp1 = new MessagePanel.MessagePanel("Sesja wygasła. Zaloguj się ponownie", false);
                                        mp1.ShowDialog();
                                        this.Close();
                                    }
                                    else
                                    {
                                        cc.sendFile(client, filename);
                                        refreshingList();
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
                            cc.sendMessage(ClientCom.ReadOrderMessage(currentFolder, token, dlg.SafeFileName), client); //client sends request for server to read file
                            if (cc.readMessage(client) == 404) //client waits for answer
                            {
                                Owner.Show();
                                MessagePanel.MessagePanel mp1 = new MessagePanel.MessagePanel("Sesja wygasła. Zaloguj się ponownie", false);
                                mp1.ShowDialog();
                                this.Close();
                            }
                            else
                            {
                                cc.sendFile(client, filename); //client sends bytes of file
                                refreshingList();
                            }
                        }
                        else
                        {
                            MessagePanel.MessagePanel mp = new MessagePanel.MessagePanel("Błąd", false);
                            mp.Show();
                        }
                    }
                    else
                    {
                        MessagePanel.MessagePanel mp = new MessagePanel.MessagePanel("Plik musi być mniejszy niż 1GB", false);
                        mp.Show();
                    }
                }
            }
        }

        //delete button event
        private void DeleteFileButton(object sender, RoutedEventArgs e)
        {
            MessagePanel.MessagePanel mp = new MessagePanel.MessagePanel("Czy jesteś pewien, że chcesz usunąć ten element?", true); //ask user if he's sure to delete
            mp.ShowDialog(); 
            if (client.Connected && mp.answear)
            {
                Button btn = ((Button)sender);
                try
                {
                    DirectoryElement temp = (DirectoryElement)btn.DataContext;
                    string deletedElement = temp.path.Replace("Main_Folder", "Main_Folder\\" + token) + temp.name;
                    cc.sendMessage(ClientCom.DeleteMessage(deletedElement, ((DirectoryElement)btn.DataContext).isFolder, token), client); //client sends request to delete file or folder
                    if (cc.readMessage(client) == 404) //client waits for answer
                    {
                        Owner.Show();
                        MessagePanel.MessagePanel mp1 = new MessagePanel.MessagePanel("Sesja wygasła. Zaloguj się ponownie", false);
                        mp1.ShowDialog();
                        this.Close();
                    }
                    else
                    {
                        refreshingList();
                        // TODO: client get a answer if file or folder was deleted
                    }
                }
                catch
                {
                    mp = new MessagePanel.MessagePanel("Coś poszło nie tak, spróbuj jeszcze raz.", false);
                    mp.Show();
                }
            }
            else
            {
                mp = new MessagePanel.MessagePanel("Błąd", false);
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
            cc.sendMessage(ClientCom.LogoutMessage(token), client);
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
                if (client.Connected)
                {
                    cc.sendMessage(ClientCom.CreateFolderMessage(currentFolder, token, cfp.folderName), client); //client sends request to server to create new folder with name of folderName parameter
                    if (cc.readMessage(client) == 404) 
                    {
                        Owner.Show();
                        MessagePanel.MessagePanel mp1 = new MessagePanel.MessagePanel("Sesja wygasła. Zaloguj się ponownie", false);
                        mp1.ShowDialog();
                        this.Close();
                    }
                    //else
                    else
                    {
                        refreshingList();
                    }
                    // TODO: client checks if its done
                }
                else
                {
                    MessagePanel.MessagePanel mp = new MessagePanel.MessagePanel("Błąd", false);
                    mp.ShowDialog();
                }
            }
        }

        //exit button event. Right now its just close the app but in the future it will be send a request to logout also in server side.
        private void ExitButton(object sender, RoutedEventArgs e)
        {
            cc.sendMessage(ClientCom.LogoutMessage(token), client);
            Owner.Close();
            this.Close();
        }

        //change password button event. Shows panel where user can change the password
        private void ChangePasswordButton(object sender, RoutedEventArgs e)
        {
            ChangePasswordPanel.ChangePasswordPanel chp = new ChangePasswordPanel.ChangePasswordPanel(cc, client, token);
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
