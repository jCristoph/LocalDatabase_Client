using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalDatabase_Client
{
    public static class Features
    {
        /// <summary>
        /// Only for Server. Client sends login parameters and now server by this Method checks if login parameters are invalid
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool LoginRecognizer(string s)
        {
            int IndexHome = s.IndexOf("<Login>") + "<Login>".Length;
            int IndexEnd = s.LastIndexOf("</Login>");
            string login = s.Substring(IndexHome, IndexEnd - IndexHome);
            IndexHome = s.IndexOf("<Pass>") + "<Pass>".Length;
            IndexEnd = s.LastIndexOf("</Pass>");
            string passowrd = s.Substring(IndexHome, IndexEnd - IndexHome);
            //tutaj funkcja sprawdzajaca haslo
            if (login.Equals("login") && passowrd.Equals("passowrd"))
                return true;
            else 
                return false;
        }
        /// <summary>
        /// Only for client usage. Client checks if his login parameters are good. If they are, change view to logged user.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool CheckLoginRecognizer(string s)
        {
            int IndexHome = s.IndexOf("<isLogged>") + "<isLogged>".Length;
            int IndexEnd = s.LastIndexOf("</isLogged>");
            if (s.Substring(IndexHome, IndexEnd - IndexHome).Equals("Yes"))
                return true;
            else
                return false;
        }
        /// <summary>
        /// For Client and Server usage. This message tells client/server to launch file downloader method.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string Download(string s)
        {
            int IndexHome = s.IndexOf("<Path>") + "<Path>".Length;
            int IndexEnd = s.LastIndexOf("</Path>");
            string path = s.Substring(IndexHome, IndexEnd - IndexHome);
            return path;
        }
        /// <summary>
        /// For Client and server usage. This message tells server/client what file has to be sent
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string Send(string s)
        {
            int IndexHome = s.IndexOf("<Path>") + "<Path>".Length;
            int IndexEnd = s.LastIndexOf("</Path>");
            string path = s.Substring(IndexHome, IndexEnd - IndexHome);
            return path;
        }
        /// <summary>
        /// For Client user. From very complex message what directory content, this method takes file data like path, name, last write date, bool is folder. 
        /// This data is put into elements list in object DirectoryManager.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static DirectoryManager SendDirectory(string data)
        {
            DirectoryManager dm = new DirectoryManager();
            foreach (var a in data.Split(new string[] { "</Task>" }, StringSplitOptions.None))
            {
                dm.ProcessPath(a);
            }
            return dm;

        }


        /// <summary>
        /// For Client usage. Client 
        /// </summary>
        /// <returns></returns>
        public static string SendDirectoryOrder()
        {
            return "<Task=SendDir></Task><#>";
        }
        /// <summary>
        /// Only For Client usage. Server sends this message and Client has to update his directory by downloading them. 
        /// </summary>
        /// <returns></returns>
        public static string UpdateDirectoryOrder()
        {
            return "";
        }
        /// <summary>
        /// For Server usage. Client sends this message and Server now has to recognize path and delete file.
        /// </summary>
        /// <param name="path"></param>
        public static void Delete(string path)
        {
            
        }
        /// <summary>
        /// For client and server usage. If something goes wrong or needs only confirmations then this method 
        /// read what to show in MessageBox.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string responseRecognizer(string s)
        {
            int IndexHome = s.IndexOf("<Path>") + "<Path>".Length;
            int IndexEnd = s.LastIndexOf("</Path>");
            return s.Substring(IndexHome, IndexEnd - IndexHome); ;
        }
    }
}
