using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace LocalDatabase_Client
{
    class ClientConnection
    {
        private String serverIP = null;
        private TextBlock text = null;
        private int port = 0;

        public ClientConnection(TextBlock text, String serverIP)
        {
            this.serverIP = serverIP;
            this.text = text;
            this.port = 25000;
        }

        public void Start()
        {
            try
            {
                Application.Current.Dispatcher.Invoke(new Action(() => { text.Text = "Waiting for a connection..."; }));
                TcpClient client = new TcpClient(serverIP, port);
                Application.Current.Dispatcher.Invoke(new Action(() => { text.Text = "Connected!!!"; }));
                //sendMessage(client);
                //downloadMessage(client);
                downloadFile(client);
                client.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e);
            }
        }
        private void recognizeMessage(string data, TcpClient client)
        {
            int taskIndexHome = data.IndexOf("<Task=") + "<Task=".Length;
            int taskIndexEnd = data.IndexOf(">");
            string task = data.Substring(taskIndexHome, taskIndexEnd - taskIndexHome);
            DirectoryManager dm;
            Application.Current.Dispatcher.Invoke(new Action(() => { text.Text = task; }));
            switch (task)
            {
                case "Login":
                    ClientCom.LoginRecognizer(data);
                    break;
                case "ChechLogin":
                    Features.CheckLoginRecognizer(data);
                    break;
                case "Logout":
                    Features.LogoutRecognizer(data);
                    break;
                case "Download": //kiedy wysylane jest zadanie pobrania pliku
                    downloadFile(client);
                    break;
                case "Send": ////kiedy wysylane jest zadanie wyslania pliku
                    sendFile(client, Features.Send(data));
                    break;
                case "SendingDir": //kiedy wysylana jest zawartosc biblioteki
                    dm = Features.SendDirectory(data);
                    Application.Current.Dispatcher.Invoke(new Action(() => { text.Text = dm.PrintFolderContent(); }));
                    break;
                case "SendDir": //kiedy wysylane jest zadanie wyslania biblioteki
                    dm = new DirectoryManager();
                    dm.ProcessDirectory(@"C:\Directory_test");
                    ClientCom.SendDirectory(dm.directoryElements);
                    break;
                case "DownloadDir":
                    downloadMessage(client);
                    break;
                case "Delete":
                    break;
                case "Response":
                    MessageBox.Show(Features.response(data));
                    break;

            }
            //DirectoryManager dm = new DirectoryManager();
            //foreach (var a in data.Split(new string[] { "</Task>" }, StringSplitOptions.None))
            //{
            //    dm.ProcessPath(a);
            //}
            //Application.Current.Dispatcher.Invoke(new Action(() => { text.Text = dm.PrintFolderContent(); }));
        }
        private void downloadMessage(TcpClient client)
        {
            var stream = client.GetStream();
            Byte[] bytes = new Byte[1024];
            int i;
            string data = "";
            int err = 0;
            do
            {
                i = stream.Read(bytes, 0, bytes.Length);
                data += Encoding.UTF8.GetString(bytes, 0, i);
                if(!stream.DataAvailable)
                    Thread.Sleep(1);
            } while (stream.DataAvailable);
            recognizeMessage(data, client);
        }
        private void sendMessage(string str, TcpClient client)
        {
            //var stream = new TcpClient("127.0.0.1", 25000).GetStream();
            var stream = new NetworkStream(client.Client);
            Byte[] reply = System.Text.Encoding.UTF8.GetBytes(str);
            stream.Write(reply, 0, reply.Length);
            stream.Close();
        }

        private void downloadFile(TcpClient client)
        {
            try
            {
                Socket handlerSocket = client.Client;
                if (handlerSocket.Connected)
                {
                    string fileName = string.Empty;
                    NetworkStream networkStream = new NetworkStream(handlerSocket);
                    int thisRead = 0;
                    int blockSize = 1024;
                    Byte[] dataByte = new Byte[blockSize];
                    lock (this)
                    {
                        Application.Current.Dispatcher.Invoke(new Action(() => { text.Text = "Downloading!!!"; }));
                        string folderPath = @"e:\";
                        handlerSocket.Receive(dataByte);
                        int fileNameLen = BitConverter.ToInt32(dataByte, 0);
                        fileName = Encoding.ASCII.GetString(dataByte, 4, fileNameLen);
                        Stream fileStream = File.OpenWrite(folderPath + fileName);
                        fileStream.Write(dataByte, 4 + fileNameLen, (1024 - (4 + fileNameLen)));
                        do
                        {
                            thisRead = networkStream.Read(dataByte, 0, blockSize);
                            fileStream.Write(dataByte, 0, thisRead);
                            if (!networkStream.DataAvailable)
                                Thread.Sleep(10);
                        } while (networkStream.DataAvailable);
                        fileStream.Close();
                    }
                    Application.Current.Dispatcher.Invoke(new Action(() => { text.Text = "Downloaded"; }));
                    handlerSocket = null;
                }
            }
            catch
            {

            }
        }
        private void sendFile(TcpClient client, string path)
        {
            string shortFileName = path;
            string longFileName = shortFileName;
            try
            {
                byte[] fileNameByte = Encoding.ASCII.GetBytes(shortFileName);
                byte[] fileData = File.ReadAllBytes(longFileName);
                byte[] clientData = new byte[4 + fileNameByte.Length + fileData.Length];
                byte[] fileNameLen = BitConverter.GetBytes(fileNameByte.Length);
                fileNameLen.CopyTo(clientData, 0);
                fileNameByte.CopyTo(clientData, 4);
                fileData.CopyTo(clientData, 4 + fileNameByte.Length);
                NetworkStream networkStream = client.GetStream();
                networkStream.Write(clientData, 0, clientData.GetLength(0));
                networkStream.Close();
            }
            catch
            {

            }
        }
    }
}
