using System;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using LocalDatabase_Client.Client;

namespace LocalDatabase_Client
{
    public class ClientConnection
    {
        private SslStream sslStream = null;
        public SslStream SslStream { get => sslStream; }
        TcpClient client = null;

        private String serverIP = null;
        public string token { get; set; } //token of logged client
        public double limit { get; set; } //limit of data space
        private int port = 0;
        private static string ServerCertificateName = "MySslSocketCertificate";

        //constructor
        public ClientConnection()
        {
            this.serverIP = SettingsManager.Instance.GetServerIp();
            this.port = 25000;
        }

        //method starts connection with server
        public void Start()
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
                sslStream.ReadTimeout = 5000; //if server doesn't respond in 5 seconds then client stop connection - it condition to avoid deadlock
                client.Connect(serverIP, port);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static X509Certificate getServerCert()
        {
            //here is needed a x509 cert 
            //in app folder must be a cert equal to server (copy of it in server and client).
            //to create cert in your system you have to open power shell as administrator and write some lines:
            ///New-SelfSignedCertificate -Subject "CN=MySslSocketCertificate" -KeySpec "Signature" -CertStoreLocation "Cert:\CurrentUser\My"
            ///dir cert:\CurrentUser\My     here you have to copy thumbprint of cert
            ///$PFXPass = ConvertTo-SecureString -String “MyPassword” -Force -AsPlainText
            ///Export-PfxCertificate -Cert cert:\CurrentUser\My\___Thumbprint_of_cert____ -Password $PFXPass -FilePath C:\Users\x509cert.pfx
            var certName = "x509cert.pfx";
            var certPassword = "MyPassword";
            return new X509Certificate2(certName, certPassword, X509KeyStorageFlags.MachineKeySet); ;
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
                        return 0;
                    case "Send": //when server sends request to send file by client
                        return 0;
                    case "SendingDir":
                        return data;
                    case "SessionExpired":
                        return 404;
                    case "AcceptTransfer":
                        return ClientCom.acceptTransferRecognizer(data);
                    case "Response":
                        if (ClientCom.responseRecognizer(data).Equals("It's ok"))
                            return 0;
                        else if (ClientCom.responseRecognizer(data).Equals("Registration success"))
                            return 3;
                        else
                            return 101;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(data + "/n" + e.ToString());
            }
            return 0;
        }

        //tcp/ip read message method. Reads bytes and translate it to string - it will be changed for ssl connection
        public dynamic readMessage(SslStream sslStream)
        {
            var inputBuffer = new byte[4096];
            StringBuilder messageData = new StringBuilder();
            var inputBytes = -1;
            do
            {
                try
                {
                    inputBytes = sslStream.Read(inputBuffer, 0, inputBuffer.Length);
                    Decoder decoder = Encoding.UTF8.GetDecoder();
                    char[] chars = new char[decoder.GetCharCount(inputBuffer, 0, inputBytes)];
                    decoder.GetChars(inputBuffer, 0, inputBytes, chars, 0);
                    messageData.Append(chars);
                    if (messageData.ToString().IndexOf("<EOM>") != -1)
                    {
                        break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    return -1;
                }
            } while (inputBytes != 0);
            sslStream.Flush();
            return recognizeMessage(messageData.ToString());
        }

        //tcp/ip send message method. translate string to bytes and send it to client by stream  - it will be changed for ssl connection
        public void sendMessage(string str, SslStream sslStream)
        {
            sslStream.Flush();
            try
            {
                var outputBuffer = Encoding.UTF8.GetBytes(str);
                sslStream.Write(outputBuffer);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                MessagePanel.MessagePanel mp = new MessagePanel.MessagePanel("Błąd", false);
                mp.ShowDialog();
            }
            sslStream.Flush();
        }
    }
}
