using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace LocalDatabase_Client
{
    public class ClientConnection
    {
        public bool isBusy { get; set;} //it will not be needed in the future. i fix it later
        private String serverIP = null;
        public string token { get; set; } //token of logged client
        public double limit { get; set; } //limit of data space
        private int port = 0;
        private static string ServerCertificateName =
   "MySslSocketCertificate";

        //constructor
        public ClientConnection(String serverIP)
        {
            isBusy = false;
            this.serverIP = serverIP;
            this.port = 25000;
        }

        //method starts connection with server
        public SslStream Start()
        {
            SslStream sslStream = null;
            TcpClient client = null;
            do
            {
                try
                {
                    var clientCertificate = getServerCert();
                    var clientCertificateCollection = new
                       X509CertificateCollection(new X509Certificate[]
                       { clientCertificate });
                    client = new TcpClient(serverIP, port);
                    sslStream = new SslStream(client.GetStream(), false, ValidateCertificate);
                    sslStream.AuthenticateAsClient(ServerCertificateName, clientCertificateCollection, SslProtocols.Tls12, false);
                    client.Connect(serverIP, port);
                }
                catch (Exception e)
                {

                }
            } while (!client.Connected); //client try to connect until it succeeded
            return sslStream;
        }

        private static X509Certificate getServerCert()
        {
            X509Store store = new X509Store(StoreName.My,
               StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly);

            X509Certificate2 foundCertificate = null;
            foreach (X509Certificate2 currentCertificate
               in store.Certificates)
            {
                if (currentCertificate.IssuerName.Name
                   != null && currentCertificate.IssuerName.
                   Name.Equals("CN=MySslSocketCertificate"))
                {
                    foundCertificate = currentCertificate;
                    break;
                }
            }
            return foundCertificate;
        }

        static bool ValidateCertificate(Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
            { return true; }
            // ignore chain errors as where self signed
            if (sslPolicyErrors ==
               SslPolicyErrors.RemoteCertificateChainErrors)
            { return true; }
            return false;
        }

        
        //a very important method where messages from server are recognized and later right method are run.
        public dynamic recognizeMessage(string data)
        {
            try
            {
                FileTransporter fileTransporter = null;
                    
                int taskIndexHome = data.IndexOf("<Task=") + "<Task=".Length;
                int taskIndexEnd = data.IndexOf(">");
                string task = data.Substring(taskIndexHome, taskIndexEnd - taskIndexHome);
                switch (task)
                {
                    case "CheckLogin":
                        string[] temp = ClientCom.CheckLoginRecognizer(data);
                        token = temp[0];
                        limit = Double.Parse(temp[1]);
                        if (token.Equals("ERROR"))
                            return 0;
                        else if (token.Equals("ERROR1"))
                            return 2;
                        else
                            return 1;
                    case "Download": //when server sends request to download file by client
                        //fileTransporter = new FileTransporter("127.0.0.1", );
                        //fileTransporter.connectAsClient();
                        //fileTransporter.recieveFile();
                        //downloadFile(client);
                        return 0;
                    case "Send": //when server sends request to send file by client
                        fileTransporter = new FileTransporter("127.0.0.1", ClientCom.SendRecognizer(data));
                        fileTransporter.connectAsClient();
                        fileTransporter.sendFile();
                        //sendFile(client, ClientCom.SendRecognizer(data));
                        return 0;
                    case "SendingDir":
                        return data;
                    case "SessionExpired":
                        return 404;
                    case "Response":
                       // MessagePanel.MessagePanel mp = new MessagePanel.MessagePanel(ClientCom.responseRecognizer(data), false);
                        //mp.Show();
                        return 0;
                }
            }
            catch (Exception e)
            {

            }

            return 0;
        }

        //tcp/ip read message method. Reads bytes and translate it to string - it will be changed for ssl connection
        public dynamic readMessage(SslStream sslStream)
        {
            var inputBuffer = new byte[4096];
            var inputBytes = 0;
            while (inputBytes == 0)
            {
                try
                {
                    inputBytes = sslStream.Read(inputBuffer, 0, inputBuffer.Length);
                }
                catch {
                    return -1;
                }

            }
            var inputMessage = Encoding.UTF8.GetString(inputBuffer,
               0, inputBytes);
            sslStream.Flush();
            return recognizeMessage(inputMessage);
        }

        //tcp/ip send message method. translate string to bytes and send it to client by stream  - it will be changed for ssl connection
        public void sendMessage(string str, SslStream sslStream)
        {
            isBusy = true;
            try
            {
                var outputBuffer = Encoding.UTF8.GetBytes(str);
                sslStream.Write(outputBuffer);
            }
            catch (Exception e)
            {
                MessagePanel.MessagePanel mp = new MessagePanel.MessagePanel("Błąd", false);
                mp.ShowDialog();
            }
            sslStream.Flush();
            isBusy = false;
        }

        //tcp/ip download method. gets bytes and sum it to create a file. From bytes read a name of file. Saves it in path chosed by user.
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
                        //handlerSocket.recieve(dataByte);
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

        //tcp/ip send file method. Read the entire file to program then bytes sends by stream.
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
