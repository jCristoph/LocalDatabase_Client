﻿using System;
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

        public TcpClient Start()
        {
            Application.Current.Dispatcher.Invoke(new Action(() => { text.Text = "Waiting for a connection..."; }));
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
            Application.Current.Dispatcher.Invoke(new Action(() => { text.Text = "Connected!!!"; }));
            return client;
        }
        public void recognizeMessage(string data, TcpClient client)
        {
            int taskIndexHome = data.IndexOf("<Task=") + "<Task=".Length;
            int taskIndexEnd = data.IndexOf(">");
            string task = data.Substring(taskIndexHome, taskIndexEnd - taskIndexHome);
            DirectoryManager dm;
            Application.Current.Dispatcher.Invoke(new Action(() => { text.Text = task; }));
            switch (task)
            {
                case "CheckLogin":
                    ClientCom.CheckLoginRecognizer(data);
                    break;
                case "Download": //kiedy wysylane jest zadanie pobrania pliku
                    downloadFile(client);
                    break;
                case "Send": ////kiedy wysylane jest zadanie wyslania pliku
                    sendFile(client, ClientCom.SendRecognizer(data));
                    break;
                case "SendingDir": //kiedy wysylana jest zawartosc biblioteki
                    dm = ClientCom.SendDirectoryRecognizer(data);
                    Application.Current.Dispatcher.Invoke(new Action(() => { text.Text = dm.PrintFolderContent(); }));
                    break;
                case "Delete":
                    break;
                case "Response":
                    //MessageBox.Show(ClientCom.responseRecognizer(data));
                    break;
            }
        }
        public void readMessage(TcpClient client)
        {
            var stream = client.GetStream();
            Byte[] bytes = new Byte[1024];
            int i;
            string data = "";
            do
            {
                i = stream.Read(bytes, 0, bytes.Length);
                data += Encoding.UTF8.GetString(bytes, 0, i);
                if(!stream.DataAvailable)
                    Thread.Sleep(1);
            } while (stream.DataAvailable);
            recognizeMessage(data, client);
        }
        public void sendMessage(string str, TcpClient client)
        {
            var stream = client.GetStream();
            Byte[] reply = System.Text.Encoding.UTF8.GetBytes(str);
            stream.Write(reply, 0, reply.Length);
        }

        public void downloadFile(TcpClient client)
        {
           // try
            //{
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
                        Application.Current.Dispatcher.Invoke(new Action(() => { text.Text = "Downloading!!!"; }));
                        string folderPath = @"e:\";
                        handlerSocket.Receive(dataByte);
                        int fileNameLen = BitConverter.ToInt32(dataByte, 0);
                        fileName = Encoding.ASCII.GetString(dataByte, 4, fileNameLen);
                        Stream fileStream = File.OpenWrite(folderPath + fileName);
                        fileStream.Write(dataByte, 4 + fileNameLen, (1024 - (4 + fileNameLen)));
                        do
                        {
                            thisRead = networkStream.Read(dataByte, 0, blockSize);//problem gdy  plik jest mniejszy od bufora
                            fileStream.Write(dataByte, 0, thisRead);
                            if (!networkStream.DataAvailable)
                                Thread.Sleep(1);
                        } while (networkStream.DataAvailable);
                        fileStream.Close();
                    }
                    Application.Current.Dispatcher.Invoke(new Action(() => { text.Text = "Downloaded"; }));
                    handlerSocket = null;
                }
            //}
            //catch
            //{

            //}
        }
        public void sendFile(TcpClient client, string path)
        {
            string shortFileName = "music.mp3";
            string longFileName = @"E:\music.mp3";
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
                //networkStream.Close();
            }
            catch
            {

            }
        }
    }
}
