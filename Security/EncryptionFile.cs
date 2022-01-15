using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;
using System.ComponentModel;

namespace LocalDatabase_Client.Security
{
    class EncryptionFile
    {
        const int BUFFER_SIZE = 4096;

        private static byte[] RandomSaltGenerator()
        {
            byte[] data = new byte[32];
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                for (int i = 0; i < 10; i++)
                    rng.GetBytes(data);
            }
            return data;
        }

        public static void Encrypt(string inFile, string userKey, BackgroundWorker helperBW)
        {
            byte[] salt = RandomSaltGenerator();
            byte[] passwordBytes = System.Text.Encoding.UTF8.GetBytes(userKey);
            int iterationsNumber = 50000;
            FileStream fsCrypt = new FileStream(inFile + ".ENC", FileMode.Create); //zaszyfrowany plik ma rozszerzenie .ENC

            RijndaelManaged AES = new RijndaelManaged();

            var key = new Rfc2898DeriveBytes(passwordBytes, salt, iterationsNumber);
            AES.Key = key.GetBytes(32);
            AES.IV = key.GetBytes(16);

            // CipherMode AES: ECB, CBC, CFB, OFB, CTR
            AES.Mode = CipherMode.CFB;

            fsCrypt.Write(salt, 0, salt.Length);

            CryptoStream cs = new CryptoStream(fsCrypt, AES.CreateEncryptor(), CryptoStreamMode.Write);
            FileStream fsIn = new FileStream(inFile, FileMode.Open);

            byte[] buffer = new byte[BUFFER_SIZE];
            int read;

            long size = fsIn.Length;
            int i = 0;

            if(helperBW == null)
            {
                while ((read = fsIn.Read(buffer, 0, buffer.Length)) > 0)
                {
                    cs.Write(buffer, 0, read);
                }
            }
            else
            {
                while ((read = fsIn.Read(buffer, 0, buffer.Length)) > 0)
                {
                    cs.Write(buffer, 0, read);
                    i = i + BUFFER_SIZE;
                    helperBW.ReportProgress((int)Math.Round((float)i / (float)size * 100));
                }
            }
 
           fsIn.Close();
           cs.Close();
           fsCrypt.Close();
        }
    }
}
