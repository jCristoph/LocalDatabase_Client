using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LocalDatabase_Client.Security
{
    class KeyHandling
    {
       public static string key = "/Yz0I0X7~GLi[9!IL$!t35&$!*O*GmIn";

        public static void safeKey(string name, string userkey)
        {
            string path = "C:\\Users\\Krzemek\\Documents\\" + name + ".dat";
            FileStream fStream;
            fStream = new FileStream(path, FileMode.Create);
            BinaryWriter binFile = new BinaryWriter(fStream);
            binFile.Write(userkey);
            binFile.Close();
            Security.EncryptionFile.Encrypt(path, KeyHandling.key);
            File.Delete(path);
        }

        public static string getKey(string name)
        {
            string userkey = null;
            FileStream fStream;
            string path = "C:\\Users\\Krzemek\\Documents\\" + name + ".dat";
            Security.DecryptionFile.Decrypt((path+".ENC"), key);
            fStream = new FileStream(path, FileMode.Open);
            BinaryReader readBinary = new BinaryReader(fStream);
            userkey = readBinary.ReadString();
            fStream.Close();
            Security.EncryptionFile.Encrypt(path, KeyHandling.key);
            File.Delete(path);
            return userkey;
        }



    }
}
