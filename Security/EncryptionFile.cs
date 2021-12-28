using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;

namespace LocalDatabase_Client.Security
{
    class EncryptionFile
    {
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

        public static void Encrypt(string inFile, string userKey)
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

            byte[] buffer = new byte[1048576];
            int read;

            while ((read = fsIn.Read(buffer, 0, buffer.Length)) > 0)
                    cs.Write(buffer, 0, read);

           fsIn.Close();

           
           cs.Close();
           fsCrypt.Close();
           Console.WriteLine("success");
        }
    }
}
