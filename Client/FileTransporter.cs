
using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Controls;


namespace LocalDatabase_Client
{
    public class FileTransporter
    {
        static int BUFFER_SIZE = 4096;

        private string ip;
        private int port;
        private FileInfo file;
        private string fileName;
        private string extension = ".ENC";
        System.Windows.Controls.ProgressBar progressBar;
        long size;
        Action refresh;
        Socket socket;

        public FileTransporter(string ip, string fileName, long size, System.Windows.Controls.ProgressBar progressBar, int port)
        {
            this.ip = ip;
            this.port = port;
            this.fileName = fileName;
            file = new FileInfo(fileName);
            this.size = size;
            this.progressBar = progressBar;

            this.progressBar.Visibility = System.Windows.Visibility.Visible;
        }

        public void connectAsServer()
        {
            IPEndPoint ipe = new IPEndPoint(IPAddress.Parse(ip), port);
            socket = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(ipe);
            socket.Listen(10);
            socket = socket.Accept();
        }

        public void connectAsClient()
        {
            IPEndPoint ipe = new IPEndPoint(IPAddress.Parse(ip), port);
            socket = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(ipe);
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
            var readed = -1;
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
                        readed = networkStream.Read(buffer, 0, buffer.Length);
                        fileStream.Write(buffer, 0, readed);
                        i = i + BUFFER_SIZE;
                        helperBW.ReportProgress((int)Math.Round((float)i / (float)size * 100));
                    }
                    catch (SocketException se)
                    {
                        Console.WriteLine(se.ToString());
                        readed = 0;
                    }
                    //If you test it on loopback better uncomment line below. Buffer is slower than loopback transfer
                    Thread.Sleep(1);
                } while (readed > (BUFFER_SIZE - 1));
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
            System.Diagnostics.Process.Start(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\");
            string key = Security.KeyHandling.getKey("ABCDFG");
            Security.DecryptionFile.Decrypt(file.FullName, key);
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
            var readed = -1;
            int i = 0;
            var buffer = new Byte[BUFFER_SIZE];
            using (var networkStream = new BufferedStream(new NetworkStream(socket, false)))
            using (var fileStream = file.OpenRead())
            {
                while (readed != 0)
                {
                    readed = fileStream.Read(buffer, 0, buffer.Length);
                    if (readed != 0)
                    {
                        try
                        {
                            networkStream.Write(buffer, 0, readed);
                            i = i + BUFFER_SIZE;
                            helperBW.ReportProgress((int)Math.Round((float)i / (float)size * 100));
                            //If you test it on loopback better uncomment line below. Buffer is slower than loopback transfer
                            Thread.Sleep(1);
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
            refresh();
        }
        #endregion


    }
}

