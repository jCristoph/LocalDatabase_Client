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
            
            string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string filePath = folderPath + "\\userKey_" + name + ".dat";
            FileStream fStream;
            fStream = new FileStream(filePath, FileMode.Create);
            BinaryWriter binFile = new BinaryWriter(fStream);
            binFile.Write(userkey);
            binFile.Close();
            Security.EncryptionFile.Encrypt(filePath, KeyHandling.key);
            File.Delete(filePath);
        }

        public static string getKey(string name)
        {
            string userkey = null;
            string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string filePath = folderPath + "\\userKey_" + name + ".dat";
            Security.DecryptionFile.Decrypt((filePath + ".ENC"), key);
            FileStream fStream = new FileStream(filePath, FileMode.Open);
            BinaryReader readBinary = new BinaryReader(fStream);
            userkey = readBinary.ReadString();
            fStream.Close();
            Security.EncryptionFile.Encrypt(filePath, KeyHandling.key);
            File.Delete(filePath);
            return userkey;
        }



    }
}
