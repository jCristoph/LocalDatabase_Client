using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
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
        private DirectoryManager directoryManager;
        private double limit;
        private ObservableCollection<DirectoryElement> currentDirectory;
        private DirectoryElement currentFolder;
        private bool isLogged;
        private string token;
        
        public MainWindow(TcpClient client, ClientConnection cc, bool isPasswordChanged)
        {
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            InitializeComponent();
            if (isPasswordChanged)
            {
                MessagePanel.MessagePanel mp = new MessagePanel.MessagePanel("Zmień hasło", false);
                mp.Show();
            }
            isLogged = true;
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
            Task t = new Task(() => refreshingList());
            t.Start();
        }

        private void refreshingList()
        {
            Thread.Sleep(10);
            while (isLogged)
            {
                if (client != null)
                {
                    if(!cc.isBusy)
                    {
                        DirectoryRequest:
                        cc.sendMessage(ClientCom.SendDirectoryOrderMessage(token), client);
                        cc.getDirectory(client, directoryManager);
                        if(directoryManager.directoryElements != null)
                        {
                            Application.Current.Dispatcher.Invoke(new Action(() => { currentDirectory.Clear(); }));
                            foreach (var a in directoryManager.directoryElements)
                            {
                                if(currentFolder.name.Equals("Udostępnione"))
                                {
                                    if (!a.pathArray[0].Contains("Main_Folder") && !a.name.Equals("Main_Folder"))
                                        Application.Current.Dispatcher.Invoke(new Action(() => { currentDirectory.Add(a); }));
                                }
                                else if (a.pathArray[a.pathArray.Count - 1] == currentFolder.name)
                                    Application.Current.Dispatcher.Invoke(new Action(() => { currentDirectory.Add(a); }));
                            }
                            Application.Current.Dispatcher.Invoke(new Action(() => { refreshTextBlock.Text = "Ostatnie odświeżenie: " + DateTime.Now;
                                                                                     sizeTextBlock.Text = "Zużyto " + Math.Round(directoryManager.usedSpace(), 2) + "GB / " + limit + "GB"; }));
                        }
                        else
                        {
                            goto DirectoryRequest;
                        }
                    }
                }
                Thread.Sleep(4*1000);
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
                        cc.sendMessage(ClientCom.SendOrderMessage((((DirectoryElement)btn.DataContext).path).Replace("Main_Folder", "Main_Folder\\" + token) + ((DirectoryElement)btn.DataContext).name, token), client);
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
            else if (btn.Content.Equals("Otwórz"))
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

        private void SendFileButton(object sender, RoutedEventArgs e)
        {
           if (client.Connected)
           {
                Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
                Nullable<bool> result = dlg.ShowDialog();
                string filename = dlg.FileName;
                if(!filename.Equals(""))
                {
                    if (new FileInfo(filename).Length < 1100000000)
                    {
                        if (currentDirectory.Any(x => x.name == dlg.SafeFileName))
                        {
                            MessagePanel.MessagePanel mp = new MessagePanel.MessagePanel("Czy na pewno chcesz nadpisać plik?", true);
                            mp.ShowDialog();
                            if (mp.answear.Equals(true))
                            {
                                if (result == true)
                                {
                                    cc.sendMessage(ClientCom.ReadOrderMessage(currentFolder, token, dlg.SafeFileName), client);
                                    cc.readMessage(client);
                                    cc.sendFile(client, filename);
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
                            cc.sendMessage(ClientCom.ReadOrderMessage(currentFolder, token, dlg.SafeFileName), client);
                            cc.readMessage(client);
                            cc.sendFile(client, filename);
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

        private void DeleteFileButton(object sender, RoutedEventArgs e)
        {
            MessagePanel.MessagePanel mp = new MessagePanel.MessagePanel("Czy jesteś pewien, że chcesz usunąć ten element?", true);
            mp.ShowDialog();
            if (client.Connected && mp.answear)
            {
                Button btn = ((Button)sender);
                DirectoryElement temp = (DirectoryElement)btn.DataContext;
                string deletedElement = temp.path.Replace("Main_Folder", "Main_Folder\\" + token) + temp.name;
                cc.sendMessage(ClientCom.DeleteMessage(deletedElement, ((DirectoryElement)btn.DataContext).isFolder, token), client);
                cc.readMessage(client);
            }
            else
            {
                mp = new MessagePanel.MessagePanel("Błąd", false);
                mp.Show();
            }
        }
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

        private void LogOutButton(object sender, RoutedEventArgs e)
        {
            //cc.sendMessage(ClientCom.LogoutMessage(), client);
            Owner.Show();
            isLogged = false;
            this.Close();
        }

        private void CreateFolderButton(object sender, RoutedEventArgs e)
        {
            CreateFolderPanel.CreateFolderPanel cfp = new CreateFolderPanel.CreateFolderPanel();
            cfp.ShowDialog();
            if(cfp.folderName != null)
            {
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
        }

        private void ExitButton(object sender, RoutedEventArgs e)
        {
            cc.sendMessage(ClientCom.LogoutMessage(), client);
            Owner.Close();
            this.Close();
        }

        private void ChangePasswordButton(object sender, RoutedEventArgs e)
        {
            ChangePasswordPanel.ChangePasswordPanel chp = new ChangePasswordPanel.ChangePasswordPanel(cc, client, token);
            chp.Show();
        }

        private void HelpButton(object sender, RoutedEventArgs e)
        {
            HelpPanel.HelpPanel hp = new HelpPanel.HelpPanel();
            hp.Show();
        }
        private void ShareFileButton(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            SharePanel.ShareFilePanel sp = new SharePanel.ShareFilePanel(cc, client, token, ((((DirectoryElement)btn.DataContext).path).Replace("Main_Folder", "Main_Folder\\" + token) + ((DirectoryElement)btn.DataContext).name));
            sp.Show();
        }
    }

}
