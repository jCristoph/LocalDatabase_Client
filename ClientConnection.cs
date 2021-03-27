using System;
using System.Collections.Generic;
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

                downloadMessage(client);
                client.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e);
            }
        }
        private void downloadMessage(TcpClient client)
        {
            var stream = client.GetStream();
            Byte[] bytes = new Byte[1024];
            int i;
            while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
            {
                string hex = BitConverter.ToString(bytes);
                string data = Encoding.UTF8.GetString(bytes, 0, i);
                Application.Current.Dispatcher.Invoke(new Action(() => { text.Text = data; }));
            }
        }
        private void sendMessage(string str, TcpClient client)
        {
            var stream = client.GetStream();
            Byte[] reply = System.Text.Encoding.UTF8.GetBytes(str);
            stream.Write(reply, 0, reply.Length);
        }
        private void downloadFile(TcpClient client)
        {
            try
            {
                while (true)
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
                            while (true)
                            {
                                thisRead = networkStream.Read(dataByte, 0, blockSize);
                                fileStream.Write(dataByte, 0, thisRead);
                                if (thisRead == 0)
                                    break;
                            }
                            fileStream.Close();
                        }
                        Application.Current.Dispatcher.Invoke(new Action(() => { text.Text = "Downloaded!!!"; }));
                        handlerSocket = null;
                    }
                }
            }
            catch
            {

            }
        }
        private void sendFile(TcpClient client)
        {
            string shortFileName = "music.mp3";
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
