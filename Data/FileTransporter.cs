using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using LocalDatabase_Client.Client;

namespace LocalDatabase_Client
{
    public class FileTransporter
    {
        const int BUFFER_SIZE = 4096;
        private string ip;
        private int port;
        private FileInfo file;
        private string fileName;
        private string token;
        private string extension = ".ENC";
        Stopwatch sw;

        System.Windows.Controls.ProgressBar progressBar;
        long size;
        Action refresh;
        Socket socket;

        public FileTransporter(string fileName, long size, System.Windows.Controls.ProgressBar progressBar, int port, string token)
        {
            this.ip = SettingsManager.Instance.GetServerIp();
            this.port = port;
            this.fileName = fileName;
            file = new FileInfo(fileName);
            this.size = size;
            this.token = token;
            this.progressBar = progressBar;
            this.progressBar.Visibility = System.Windows.Visibility.Visible;
            sw = new Stopwatch();
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
            string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\";
            file = new FileInfo(folderPath + fileName + extension);
            BackgroundWorker helperBW = sender as BackgroundWorker;
            helperBW.ReportProgress(0);
            var read = -1;
            var buffer = new Byte[BUFFER_SIZE];
            int i = 0;
            sw.Start();
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
        }
        private void recieveFile_bg_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
        }
        private void recieveFile_bg_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
            progressBar.Visibility = System.Windows.Visibility.Hidden;
            string key = Security.KeyHandling.GetKey(token);
            Security.DecryptionFile.Decrypt(file.FullName, key);
            System.Diagnostics.Process.Start(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\");
            sw.Stop();
            Console.WriteLine("Time spend since click button: " + sw.ElapsedMilliseconds);
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
            var read = -1;
            int i = 0;
            var buffer = new Byte[BUFFER_SIZE];
            sw.Start();
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
            progressBar.Visibility = System.Windows.Visibility.Hidden;
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
            Thread.Sleep(200);
            refresh();
            File.Delete(file.FullName); //delete encryption file after sending
            sw.Stop();
            Console.WriteLine("Time spend since click button: " + sw.ElapsedMilliseconds);
            sw.Reset();
        }
        #endregion

    }
}

