using System;
using System.IO;

namespace LocalDatabase_Client.Security
{
    class KeyHandling
    {
       public static string key = "/Yz0I0X7~GLi[9!IL$!t35&$!*O*GmIn";

        public static void SaveKey(string name, string userkey)
        {
            string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string filePath = folderPath + "\\userKey_" + name + ".dat";
            FileStream fStream;
            fStream = new FileStream(filePath, FileMode.Create);
            BinaryWriter binFile = new BinaryWriter(fStream);
            binFile.Write(userkey);
            binFile.Close();
            Security.EncryptionFile.Encrypt(filePath, KeyHandling.key, null);
            File.Delete(filePath);
        }

        public static string GetKey(string name)
        {
            string userKey = null;
            string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string filePath = folderPath + "\\userKey_" + name + ".dat";
            Security.DecryptionFile.Decrypt((filePath + ".ENC"), key, null);
            FileStream fStream = new FileStream(filePath, FileMode.Open);
            BinaryReader readBinary = new BinaryReader(fStream);
            userKey = readBinary.ReadString();
            fStream.Close();
            Security.EncryptionFile.Encrypt(filePath, KeyHandling.key, null);
            File.Delete(filePath);
            return userKey;
        }
    }
}
