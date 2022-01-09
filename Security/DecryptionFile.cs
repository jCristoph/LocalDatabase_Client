﻿using System.IO;
using System.Security.Cryptography;

namespace LocalDatabase_Client.Security
{
    public class DecryptionFile
    {
        public static void Decrypt(string inFile, string userKey)
        {
            byte[] passwordBytes = System.Text.Encoding.UTF8.GetBytes(userKey);
            byte[] salt = new byte[32];
            int iterationsNumber = 50000;

            FileStream fsCrypt = new FileStream(inFile, FileMode.Open);
            fsCrypt.Read(salt, 0, salt.Length);

            RijndaelManaged AES = new RijndaelManaged();
            var key = new Rfc2898DeriveBytes(passwordBytes, salt, iterationsNumber);
            AES.Key = key.GetBytes(32); //key size
            AES.IV = key.GetBytes(16); //block size
            AES.Padding = PaddingMode.PKCS7;

            //CipherMode AES: ECB, CBC, CFB, OFB, CTR
            AES.Mode = CipherMode.CFB;

            string dirName = Path.GetDirectoryName(inFile);
            string fileName = Path.GetFileNameWithoutExtension(inFile);
            string fileExtension = Path.GetExtension(inFile);
            
            CryptoStream cs = new CryptoStream(fsCrypt, AES.CreateDecryptor(), CryptoStreamMode.Read);
            FileStream fsOut = new FileStream(dirName + "\\" + fileName, FileMode.Create);

            int read;
            byte[] buffer = new byte[4096];

            while ((read = cs.Read(buffer, 0, buffer.Length)) > 0)
                fsOut.Write(buffer, 0, read);

            cs.Close();

            File.Delete(@dirName + "\\" + fileName + fileExtension);
            fsOut.Close();
            fsCrypt.Close();
        }
    }
}