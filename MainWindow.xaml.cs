using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        
        public MainWindow(TcpClient client, ClientConnection cc)
        {
            
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            InitializeComponent();
            this.client = client;
            this.cc = cc;
            currentDirectory = new ObservableCollection<DirectoryElement>();
            currentFolder = new DirectoryElement("\\Main_Folder", 0, "None", true);
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
                        cc.sendMessage(ClientCom.SendDirectoryOrderMessage(), client);
                        directory = cc.getDirectory(client);
                        Application.Current.Dispatcher.Invoke(new Action(() => { currentDirectory.Clear(); }));
                        foreach(var a in directory)
                        {
                            if (a.pathArray[a.pathArray.Count -1] == currentFolder.name)
                                Application.Current.Dispatcher.Invoke(new Action(() => { currentDirectory.Add(a); }));
                        }
                        Application.Current.Dispatcher.Invoke(new Action(() => { refreshTextBlock.Text = "Ostatnie odświeżenie: " + DateTime.Now; }));
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
                        cc.sendMessage(ClientCom.SendOrderMessage(((DirectoryElement)btn.DataContext).path + ((DirectoryElement)btn.DataContext).name), client);
                        cc.downloadFile(client);
                    }
                    catch (Exception err)
                    {
                        MessageBox.Show("Wybierz plik do pobrania!");
                    }
                }
                else
                    MessageBox.Show("Error");
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
            }
            else
                MessageBox.Show("Error");
           
        }

        private void SendFileButton(object sender, RoutedEventArgs e)
        {
           if (client.Connected)
           {
                Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
                Nullable<bool> result = dlg.ShowDialog();
                string filename = dlg.FileName;
                if (result == true)
                {
                    cc.sendMessage(ClientCom.ReadOrderMessage(), client);
                    cc.readMessage(client);
                    cc.sendFile(client, filename);
                }
                else
                    MessageBox.Show("Error");
            }
            else
                MessageBox.Show("Error");
        }

        private void DeleteFileButton(object sender, RoutedEventArgs e)
        {
            if (client.Connected)
            {
                Button btn = ((Button)sender);
                string deletedElement = ((DirectoryElement)btn.DataContext).path + ((DirectoryElement)btn.DataContext).name;
                cc.sendMessage(ClientCom.DeleteMessage(deletedElement), client);
                cc.readMessage(client);
            }
            else
                MessageBox.Show("Error");
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
                MessageBox.Show("Jesteś w głównym folderze");
        }
    }

}
