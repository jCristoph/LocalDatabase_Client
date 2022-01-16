using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using LocalDatabase_Client.Client;

namespace LocalDatabase_Client
{
    public class FileTransporter
    {
        private const int BUFFER_SIZE = 4096;
        private const string extension = ".ENC";

        private string ip;
        private int port;
        private string token;
        Socket socket;

        private FileInfo file;
        private string fileName;
        private long size;
        
        Stopwatch sw;

        System.Windows.Controls.ProgressBar progressBar;
        System.Windows.Controls.TextBlock progressBarStatus;
        System.Windows.Controls.Grid progressBarGrid;

        Action refresh;
        

        public FileTransporter(string fileName, long size, System.Windows.Controls.Grid progressBarGrid, int port, string token)
        {
            sw = new Stopwatch();
            sw.Start();
            this.fileName = fileName + extension;
            this.ip = SettingsManager.Instance.GetServerIp();
            this.port = port;
            file = new FileInfo(this.fileName);
            this.size = size;
            this.token = token;

            progressBarGrid.Visibility = System.Windows.Visibility.Visible;
            this.progressBarGrid = progressBarGrid;
            this.progressBar = progressBarGrid.Children[0] as System.Windows.Controls.ProgressBar;
            this.progressBarStatus = progressBarGrid.Children[1] as System.Windows.Controls.TextBlock;
        }

        public void connectAsClient()
        {
            IPEndPoint ipe = new IPEndPoint(IPAddress.Parse(ip), port);
            socket = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                socket.Connect(ipe);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        #region recieve File Asynchronous
        public void recieveFile(Action refresh)
        {
            this.refresh = refresh;
            var recieveFile_bg = new BackgroundWorker();
            recieveFile_bg.DoWork += new DoWorkEventHandler(recieveFile_bg_DoWork);
            recieveFile_bg.RunWorkerCompleted += new RunWorkerCompletedEventHandler(recieveFile_bg_RunWorkerCompleted);
            recieveFile_bg.ProgressChanged += recieveFile_bg_ProgressChanged;
            recieveFile_bg.WorkerSupportsCancellation = true;
            recieveFile_bg.WorkerReportsProgress = true;
            recieveFile_bg.RunWorkerAsync();
        }

        private void recieveFile_bg_DoWork(object sender, DoWorkEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(new Action(() => { progressBarStatus.Text = "Download"; }));

            string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\";
            file = new FileInfo(folderPath + fileName);
            BackgroundWorker helperBW = sender as BackgroundWorker;
            helperBW.ReportProgress(0);
            var read = -1;
            var buffer = new Byte[BUFFER_SIZE];
            int i = 0;
            using (var fileStream = file.OpenWrite())
            using (var networkStream = new NetworkStream(socket, false))
            {
                networkStream.ReadTimeout = 10000;
                do
                {
                    try
                    {
                        read = networkStream.Read(buffer, 0, buffer.Length);
                        fileStream.Write(buffer, 0, read);
                        i = i + BUFFER_SIZE;
                        helperBW.ReportProgress((int)Math.Round((float)i / (float)size * 100));
                    }
                    catch (SocketException se)
                    {
                        Console.WriteLine(se.ToString());
                        read = 0;
                    }
                } while (read != 0);
                networkStream.Close();
            }
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                progressBar.Value = 0; 
                progressBarStatus.Text = "Decryption"; 
            }));

            string key = Security.KeyHandling.GetKey(token);
            Security.DecryptionFile.Decrypt(file.FullName, key, helperBW);
            System.Diagnostics.Process.Start(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\");
            
        }
        private void recieveFile_bg_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
        }
        private void recieveFile_bg_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();

            progressBarGrid.Visibility = System.Windows.Visibility.Hidden;

            sw.Stop();
            Console.WriteLine("Time spend since click button \"Download\": " + sw.ElapsedMilliseconds);
            sw.Reset();
        }
        #endregion

        #region Send File Asynchronous
        public void sendFile(Action refresh)
        {
            this.refresh = refresh;

            var sendFile_bg = new BackgroundWorker();
            sendFile_bg.DoWork += new DoWorkEventHandler(sendFile_bg_DoWork);
            sendFile_bg.RunWorkerCompleted += new RunWorkerCompletedEventHandler(sendFile_bg_RunWorkerCompleted);
            sendFile_bg.ProgressChanged += sendFile_bg_ProgressChanged;
            sendFile_bg.WorkerSupportsCancellation = true;
            sendFile_bg.WorkerReportsProgress = true;
            sendFile_bg.RunWorkerAsync();
        }

        private void sendFile_bg_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker helperBW = sender as BackgroundWorker;
            helperBW.ReportProgress(0);

            Application.Current.Dispatcher.Invoke(new Action(() => { progressBarStatus.Text = "Encryption"; }));
            
            //tu następuje zaszyfrowanie pliku
            string key = Security.KeyHandling.GetKey(token);
            Security.EncryptionFile.Encrypt(fileName.Replace(extension,""), key, helperBW);

            Application.Current.Dispatcher.Invoke(new Action(() => { progressBarStatus.Text = "Upload"; }));

            var read = -1;
            int i = 0;
            var buffer = new Byte[BUFFER_SIZE];
            
            using (var networkStream = new BufferedStream(new NetworkStream(socket, false)))
            using (var fileStream = file.OpenRead())
            {
                while (read != 0)
                {
                    read = fileStream.Read(buffer, 0, buffer.Length);
                    if (read != 0)
                    {
                        try
                        {
                            networkStream.Write(buffer, 0, read);
                            i = i + BUFFER_SIZE;
                            helperBW.ReportProgress((int)Math.Round((float)i / (float)size * 100));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                            return;
                        }
                    }
                }
                buffer = new byte[BUFFER_SIZE];
                networkStream.Close();
            }
        }
        private void sendFile_bg_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
        }
        private void sendFile_bg_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            progressBarGrid.Visibility = System.Windows.Visibility.Hidden;
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
            refresh();
            File.Delete(file.FullName); //delete encryption file after sending

            sw.Stop();
            Console.WriteLine("Time spend since click button \"Upload\": " + sw.ElapsedMilliseconds);
            sw.Reset();
        }
        #endregion

    }
}

