using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public class ClientConnection
    {
        public bool isBusy { get; set;}
        private String serverIP = null;
        public string token { get; set; }
        public double limit { get; set; }
        private int port = 0;

        public ClientConnection(String serverIP)
        {
            isBusy = false;
            this.serverIP = serverIP;
            this.port = 25000;
        }

        public TcpClient Start()
        {
            TcpClient client = new TcpClient();
            do
            {
                try
                {
                    client.Connect(serverIP, port);
                }
                catch (Exception e)
                {

                }
            } while (!client.Connected);
            return client;
        }
        public void getDirectory(TcpClient client, DirectoryManager directoryManager)
        {
            if(!isBusy)
            {
                isBusy = true;
                var stream = client.GetStream();
                Byte[] bytes = new Byte[1024];
                int i;
                string data = "";
                Thread.Sleep(1000);
                do
                {
                    i = stream.Read(bytes, 0, bytes.Length);
                    data += Encoding.UTF8.GetString(bytes, 0, i);
                    if (!stream.DataAvailable)
                        Thread.Sleep(1);
                } while (stream.DataAvailable);
                isBusy = false;
                int taskIndexHome = data.IndexOf("<Task=") + "<Task=".Length;
                int taskIndexEnd = data.IndexOf(">");
                if (taskIndexEnd - taskIndexHome <= 0)
                {
                    
                }
                else
                {
                    string task = data.Substring(taskIndexHome, taskIndexEnd - taskIndexHome);
                    DirectoryManager dm;
                    Application.Current.Dispatcher.Invoke(new Action(() => { directoryManager.directoryElements.Clear(); })); 
                    if (task.Equals("SendingDir"))
                    {
                        ClientCom.SendDirectoryRecognizer(data, directoryManager);
                    }
                }
            }
        }
        

        public int recognizeMessage(string data, TcpClient client)
        {
            try
            {
                int taskIndexHome = data.IndexOf("<Task=") + "<Task=".Length;
                int taskIndexEnd = data.IndexOf(">");
                string task = data.Substring(taskIndexHome, taskIndexEnd - taskIndexHome);
                DirectoryManager dm;
                switch (task)
                {
                    case "CheckLogin":
                        string[] temp = ClientCom.CheckLoginRecognizer(data);
                        token = temp[0];
                        limit = Double.Parse(temp[1]);
                        if (token.Equals("ERROR"))
                            return 0;
                        else
                            return 1;
                    case "Download": //kiedy wysylane jest zadanie pobrania pliku
                        downloadFile(client);
                        return 0;
                    case "Send": ////kiedy wysylane jest zadanie wyslania pliku
                        sendFile(client, ClientCom.SendRecognizer(data));
                        return 0;
                    case "Response":
                       // MessagePanel.MessagePanel mp = new MessagePanel.MessagePanel(ClientCom.responseRecognizer(data), false);
                        //mp.Show();
                        return 0;
                }
            }
            catch
            {

            }

            return 0;
        }
        public int readMessage(TcpClient client)
        {
            isBusy = true;
            var stream = client.GetStream();
            Byte[] bytes = new Byte[1024];
            int i;
            string data = "";
           // Thread.Sleep(10);
            do
            {
                i = stream.Read(bytes, 0, bytes.Length);
                data += Encoding.UTF8.GetString(bytes, 0, i);
                if(!stream.DataAvailable)
                    Thread.Sleep(1);
            } while (stream.DataAvailable) ;
                isBusy = false;
            return recognizeMessage(data, client);
        }
        public void sendMessage(string str, TcpClient client)
        {
            isBusy = true;
            try
            {
                var stream = client.GetStream();
                Byte[] reply = System.Text.Encoding.UTF8.GetBytes(str);
                stream.Write(reply, 0, reply.Length);
            }
            catch (Exception e)
            {
                MessagePanel.MessagePanel mp = new MessagePanel.MessagePanel("Błąd", false);
                mp.ShowDialog();
            }
            isBusy = false;
        }

        public void downloadFile(TcpClient client)
        {
            isBusy = true;
            try
            {
                client.GetStream().Flush();
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
                        string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\";
                        handlerSocket.Receive(dataByte);
                        int fileNameLen = BitConverter.ToInt32(dataByte, 0);
                        fileName = Encoding.UTF8.GetString(dataByte, 4, fileNameLen);
                        Stream fileStream = File.OpenWrite(folderPath + fileName);
                        fileStream.Write(dataByte, 4 + fileNameLen, (1024 - (4 + fileNameLen)));
                        //ProgressBar.ProgressBar p = new ProgressBar.ProgressBar();
                        //p.Show();
                        while (networkStream.DataAvailable)
                        {
                            //p.progress++;
                            thisRead = networkStream.Read(dataByte, 0, blockSize);
                            fileStream.Write(dataByte, 0, thisRead);
                            if (!networkStream.DataAvailable)
                                Thread.Sleep(1);
                        } 
                        //p.Close();
                        fileStream.Close();
                    }
                    handlerSocket = null;
                }
            }
            catch (Exception e)
            {
                MessagePanel.MessagePanel mp = new MessagePanel.MessagePanel(e.ToString(), false);
                mp.ShowDialog(); 
            }
            isBusy = false;
        }
        public void sendFile(TcpClient client, string path)
        {
            isBusy = true;
            int IndexHome = path.LastIndexOf("\\") + "\\".Length;
            int IndexEnd = path.Length;
            string shortFileName = path.Substring(IndexHome, IndexEnd - IndexHome);
            string longFileName = path;
            try
            {
                byte[] fileNameByte = Encoding.UTF8.GetBytes(shortFileName);
                byte[] fileData = File.ReadAllBytes(longFileName);
                byte[] clientData = new byte[4 + fileNameByte.Length + fileData.Length];
                byte[] fileNameLen = BitConverter.GetBytes(fileNameByte.Length);
                fileNameLen.CopyTo(clientData, 0);
                fileNameByte.CopyTo(clientData, 4);
                fileData.CopyTo(clientData, 4 + fileNameByte.Length);
                NetworkStream networkStream = new NetworkStream(client.Client);
                networkStream.Write(clientData, 0, clientData.GetLength(0));
                networkStream.Close();
            }
            catch
            {

            }
            isBusy = false;
        }
    }
}
