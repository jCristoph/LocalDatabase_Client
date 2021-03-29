using System;
using System.Collections.Generic;
using System.Windows;

namespace LocalDatabase_Client
{
    public static class ClientCom
    {
        #region type of messages
        /// <summary>
        /// For Client  usage. This message is sent when user try to log in with parameters.
        /// </summary>
        /// <param name="login"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string LoginMessage(string login, string password)
        {
            return "<Task=Login><Login>" + login + "</Login><Pass>" + password + "</Pass></Task><#>";
        }

        /// <summary>
        /// For client usage. Sent when user ends work.
        /// </summary>
        /// <returns></returns>
        public static string LogoutMessage()
        {
            return "<Task=Logout></Task><#>";
        }

        /// <summary>
        /// For both. Send to other order that he has to listen for file
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string SendOrderMessage(string path)
        {
            return "<Task=Send><Path>" + path + "</Path></Task><#>";
        }

        /// <summary>
        /// For both. Send to other order to send file of this path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string ReadOrderMessage()
        {
            return "<Task=ReadOrder></Task><#>";
        }

        /// <summary>
        /// For Client usage. Is order for server to send directory.
        /// </summary>
        /// <returns></returns>
        public static string SendDirectoryOrderMessage()
        {
            return "<Task=SendDir></Task><#>";
        }

        /// <summary>
        /// For Client Usage. Is order to delete from server file of param path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string DeleteMessage(string path)
        {
            return "<Task=Delete><Path>" + path + "</Path></Task><#>";
        }

        /// <summary>
        /// For client and server usage. If something goes wrong or needs only confirmations then this method
        /// send what to show in other side MessageBox.
        /// </summary>
        /// <param name="goesWrong"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string ResponseMessage(bool goesWrong, string content)
        {
            return "<Task=Response><Content>" + content + "</Content></Task><#>";
        }
        #endregion

        #region message recognizer methods



        /// <summary>
        /// Only for client usage. Client checks if his login parameters are good. If they are, change view to logged user.
        /// param s is message sent from Server ie. <Task=CheckLogin><isLogged>Yes</isLogged><Login>
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static void CheckLoginRecognizer(string s)
        {
            int IndexHome = s.IndexOf("<isLogged>") + "<isLogged>".Length;
            int IndexEnd = s.LastIndexOf("</isLogged>");
            if (s.Substring(IndexHome, IndexEnd - IndexHome).Equals("Yes"))
                MessageBox.Show("You are logged in!");
            else
                MessageBox.Show("Your login or password is valid!");
        }

        /// <summary>
        /// For Client and Server usage. This message tells client/server to launch file downloader method.
        /// param s is message sent from other like "<Task=Download></Task><#>"
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string DownloadRecognizer(string s)
        {
            return "";
        }

        /// <summary>
        /// For Client and server usage. This message tells server/client what file has to be sent
        /// param s is message from other ie. <Task=Send><Path>path</Path></Task><#>
        /// Right here we are launching sendFile method.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string SendRecognizer(string s)
        {
            int IndexHome = s.IndexOf("<Path>") + "<Path>".Length;
            int IndexEnd = s.LastIndexOf("</Path>");
            string path = s.Substring(IndexHome, IndexEnd - IndexHome);
            return path;
        }

        /// <summary>
        /// For Client user. From very complex message what directory content, this method takes file data like path, name, last write date, bool is folder. 
        /// This data is put into elements list in object DirectoryManager.
        /// param s is message with all directory sent from server ie. \
        /// "<Task=SendingDir><Folder>True</Folder><Path>path</Path><Name>name</Name><Size>size</Size><Last Write>lwr</Last Write></Task>...;
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static DirectoryManager SendDirectoryRecognizer(string data)
        {
            DirectoryManager dm = new DirectoryManager();
            foreach (var a in data.Split(new string[] { "</Task>" }, StringSplitOptions.None))
            {
                dm.ProcessPath(a);
            }
            return dm;

        }

        /// <summary>
        /// Only For Client usage. Server sends this message and Client has to update his directory by downloading them. 
        /// </summary>
        /// <returns></returns>
        public static string UpdateDirectoryOrderRecognizer()
        {
            return "";
        }

        /// <summary>
        /// For client and server usage. If something goes wrong or needs only confirmations then this method 
        /// read what to show in MessageBox.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string responseRecognizer(string s)
        {
            int IndexHome = s.IndexOf("<Content>") + "<Content>".Length;
            int IndexEnd = s.LastIndexOf("</Content>");
            return s.Substring(IndexHome, IndexEnd - IndexHome); ;
        }
        #endregion
    }
}
