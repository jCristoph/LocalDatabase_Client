using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LocalDatabase_Client
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TcpClient client;
        private ClientConnection cc;
        private ObservableCollection<DirectoryElement> directory;
        private ObservableCollection<DirectoryElement> currentDirectory;
        private DirectoryElement currentFolder;
        private string token;
        
        public MainWindow(TcpClient client, ClientConnection cc, bool isPasswordChanged)
        {
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            InitializeComponent();
            if (isPasswordChanged)
            {
                MessagePanel.MessagePanel mp = new MessagePanel.MessagePanel("Zmień hasło", false);
                mp.ShowDialog();
            }
            token = cc.token;
            this.client = client;
            this.cc = cc;
            currentDirectory = new ObservableCollection<DirectoryElement>();
            currentFolder = new DirectoryElement("\\Main_Folder", 0, "None", true);
            currentFolderTextBlock.Text = currentFolder.path + currentFolder.name;
            listView.ItemsSource = currentDirectory;
            DataContext = this;
            Task t = new Task(() => refreshingList());
            t.Start();
        }

        private void refreshingList()
        {
            Thread.Sleep(10);
            while (true)
            {
                if (client != null)
                {
                    if(!cc.isBusy)
                    {
                        DirectoryRequest:
                        cc.sendMessage(ClientCom.SendDirectoryOrderMessage(token), client);
                        directory = cc.getDirectory(client);
                        if(directory != null)
                        {
                            Application.Current.Dispatcher.Invoke(new Action(() => { currentDirectory.Clear(); }));
                            foreach (var a in directory)
                            {
                                if (a.pathArray[a.pathArray.Count - 1] == currentFolder.name)
                                    Application.Current.Dispatcher.Invoke(new Action(() => { currentDirectory.Add(a); }));
                            }
                            Application.Current.Dispatcher.Invoke(new Action(() => { refreshTextBlock.Text = "Ostatnie odświeżenie: " + DateTime.Now; }));
                        }
                        else
                        {
                            goto DirectoryRequest;
                        }
                    }
                }
                Thread.Sleep(5*1000);
            }
        }


        private void DownloadOrOpenButton(object sender, RoutedEventArgs e)
        {
            Button btn = ((Button)sender);
            if (btn.Content.Equals("Pobierz"))
            {
                if (client.Connected)
                {
                    try
                    {
                        cc.sendMessage(ClientCom.SendOrderMessage((((DirectoryElement)btn.DataContext).path).Replace("Main_Folder", "Main_Folder\\" + token) + ((DirectoryElement)btn.DataContext).name), client);
                        cc.downloadFile(client);
                    }
                    catch (Exception err)
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
            else if (btn.Content.Equals("Otwórz"))
            {
                currentFolder = ((DirectoryElement)btn.DataContext);
                currentDirectory.Clear();
                foreach (var a in directory)
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

        private void SendFileButton(object sender, RoutedEventArgs e)
        {
           if (client.Connected)
           {
                Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
                Nullable<bool> result = dlg.ShowDialog();
                string filename = dlg.FileName;
                if (currentDirectory.Any(x => x.name == dlg.SafeFileName))
                {
                    MessagePanel.MessagePanel mp = new MessagePanel.MessagePanel("Czy na pewno chcesz nadpisać plik?", true);
                    mp.ShowDialog();
                    if(mp.answear.Equals(true))
                    {
                        if (result == true)
                        {
                            cc.sendMessage(ClientCom.ReadOrderMessage(currentFolder, token), client);
                            cc.readMessage(client);
                            cc.sendFile(client, filename);
                        }
                        else
                        {
                            mp = new MessagePanel.MessagePanel("Błąd", false);
                            mp.ShowDialog();
                        }
                    }
                }
                else if (result == true)
                {
                    cc.sendMessage(ClientCom.ReadOrderMessage(currentFolder, token), client);
                    cc.readMessage(client);
                    cc.sendFile(client, filename);
                }
                else
                {
                    MessagePanel.MessagePanel mp = new MessagePanel.MessagePanel("Błąd", false);
                    mp.ShowDialog();
                }
            }
            else
            {
                MessagePanel.MessagePanel mp = new MessagePanel.MessagePanel("Błąd", false);
                mp.ShowDialog();
            }
        }

        private void DeleteFileButton(object sender, RoutedEventArgs e)
        {
            MessagePanel.MessagePanel mp = new MessagePanel.MessagePanel("Czy jesteś pewien, że chcesz usunąć ten element?", true);
            mp.ShowDialog();
            if (client.Connected && mp.answear)
            {
                Button btn = ((Button)sender);
                string deletedElement = (((DirectoryElement)btn.DataContext).path).Replace("Main_Folder", "Main_Folder\\" + token) + ((DirectoryElement)btn.DataContext).name;
                cc.sendMessage(ClientCom.DeleteMessage(deletedElement, ((DirectoryElement)btn.DataContext).isFolder), client);
                cc.readMessage(client);
            }
            else
            {
                mp = new MessagePanel.MessagePanel("Błąd", false);
                mp.ShowDialog();
            }
        }
        private void ReturnButton(object sender, RoutedEventArgs e)
        {
            if (!currentFolder.name.Equals("Main_Folder"))
            {
                currentFolder = directory.First(x => x.name.Equals(currentFolder.pathArray[currentFolder.pathArray.Count - 1]));
                currentDirectory.Clear();
                foreach (var a in directory)
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

        private void LogOutButton(object sender, RoutedEventArgs e)
        {
            cc.sendMessage(ClientCom.LogoutMessage(), client);
            Owner.Show();
            this.Close();
        }

        private void CreateFolderButton(object sender, RoutedEventArgs e)
        {
            CreateFolderPanel.CreateFolderPanel cfp = new CreateFolderPanel.CreateFolderPanel();
            cfp.ShowDialog();
            if (client.Connected)
            {
                cc.sendMessage(ClientCom.CreateFolderMessage(currentFolder, token, cfp.folderName), client);
                cc.readMessage(client);
            }
            else
            {
                MessagePanel.MessagePanel mp = new MessagePanel.MessagePanel("Błąd", false);
                mp.ShowDialog();
            }
        }

        private void ExitButton(object sender, RoutedEventArgs e)
        {
            cc.sendMessage(ClientCom.LogoutMessage(), client);
            Owner.Close();
            this.Close();
        }

        private void ChangePasswordButton(object sender, RoutedEventArgs e)
        {

        }

        private void HelpButton(object sender, RoutedEventArgs e)
        {

        }
        private void ShareFileButton(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            SharePanel.ShareFilePanel sp = new SharePanel.ShareFilePanel(cc, client, token, ((((DirectoryElement)btn.DataContext).path).Replace("Main_Folder", "Main_Folder\\" + token) + ((DirectoryElement)btn.DataContext).name));
            sp.Show();
        }
    }

}
