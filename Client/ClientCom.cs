using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

/// <summary>
/// Client Com is a static class which creates a ready to send messages for server. Also it is needed for recognizing messages from server
/// </summary>

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

        public static string ChangePasswordMessage(string newPassword, string token)
        {
            return "<Task=ChngPass><NewPass>" + newPassword + "</NewPass><Token>" + token + "</Token></Task>";
        }

        /// <summary>
        /// For client usage. Sent when user ends work.
        /// </summary>
        /// <returns></returns>
        public static string LogoutMessage(string token)
        {
            return "<Task=Logout><Token>" + token + "</Token></Task><#>";
        }

        /// <summary>
        /// For both. Send to other order that he has to listen for file
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string SendOrderMessage(string path, string token)
        {
            return "<Task=Send><Path>" + path + "</Path><Token>" + token + "</Token></Task><#>";
        }

        /// <summary>
        /// For both. Send to other order to send file of this path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string ReadOrderMessage(DirectoryElement currentFolderPath, string token, string filename)
        {
            if (currentFolderPath.path.Equals("\\"))
            {
                return "<Task=ReadOrder><Path>Main_Folder\\" + token + "</Path><Token>" + token + "</Token><Name>" + filename + "</Name></Task><#>";
            }
            else
            {
                return "<Task=ReadOrder><Path>" + currentFolderPath.path.Replace("Main_Folder", "Main_Folder\\" + token) + "\\" + currentFolderPath.name + "</Path><Token>" + token + "</Token><Name>" + filename + "</Name></Task><#>";
            }
        }

        /// <summary>
        /// A method that sends a message with task for server to create a new folder
        /// </summary>
        /// <param name="currentFolderPath"> it is needed to know where new folder has to be created</param>
        /// <param name="token"> it is needed to know witch directory use (where looking for a path) </param>
        /// <param name="newFolderName"> name of new folder </param>
        /// <returns></returns>
        public static string CreateFolderMessage(DirectoryElement currentFolderPath, string token, string newFolderName)
        {
            if (currentFolderPath.path.Equals("\\"))
            {
                return "<Task=CreateFolder><Token>" + token + "</Token><Path>Main_Folder\\" + token + "\\" + newFolderName + "</Path></Task><#>";
            }
            else
            {
                return "<Task=CreateFolder><Token>" + token + "</Token><Path>" + currentFolderPath.path.Replace("Main_Folder", "Main_Folder\\" + token) + "\\" + currentFolderPath.name + "\\" + newFolderName + "</Path></Task><#>";
            }
        }
        /// <summary>
        /// For Client usage. Is order for server to send directory.
        /// </summary>
        /// <returns></returns>
        public static string SendDirectoryOrderMessage(string token)
        {
            return "<Task=SendDir><Token>" + token + "</Token></Task><#>";
        }

        /// <summary>
        /// For Client Usage. Is order to delete from server file of param path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string DeleteMessage(string path, bool isFolder, string token)
        {
            return "<Task=Delete><Path>" + path + "</Path><isFolder>" + isFolder.ToString() + "</isFolder><Token>" + token + "</Token></Task><#>";
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
        public static string[] CheckLoginRecognizer(string s)
        {
            int IndexHome = s.IndexOf("<isLogged>") + "<isLogged>".Length;
            int IndexEnd = s.LastIndexOf("</isLogged>");
            string token = s.Substring(IndexHome, IndexEnd - IndexHome);
            IndexHome = s.IndexOf("<Limit>") + "<Limit>".Length;
            IndexEnd = s.LastIndexOf("</Limit>");
            string limit = s.Substring(IndexHome, IndexEnd - IndexHome);
            if (token.Equals("ERROR"))
                return new string[] { "ERROR", "0"};
            else if (token.Equals("ERROR1"))
                return new string[] { "ERROR1", "0" };
            else
                return new string[] { token, limit}; ;
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
        public static void SendDirectoryRecognizer(string data, DirectoryManager directoryManager)
        {
            DirectoryManager dm = new DirectoryManager();
           Application.Current.Dispatcher.Invoke(new Action(() => { directoryManager.directoryElements.Clear(); })); 
            foreach (var a in data.Split(new string[] { "</Task>" }, StringSplitOptions.None))
            {
                dm.ProcessPath(a);
                directoryManager.ProcessPath(a);
            }
            DirectoryElement de = new DirectoryElement(@"Main_Folder\Udostępnione", 0, "None", true);
            Application.Current.Dispatcher.Invoke(new Action(() => { directoryManager.directoryElements.Add(de); })); 
            dm.directoryElements.Add(de);
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
