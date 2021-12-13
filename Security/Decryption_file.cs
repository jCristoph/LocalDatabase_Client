using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;


namespace LocalDatabase_Client.Security
{
    public class Decryption_file
    {

        public static void Decrypt(string inFile, string user_key)
        {
            byte[] passwordBytes = System.Text.Encoding.UTF8.GetBytes(user_key);
            byte[] salt = new byte[32];

            FileStream fsCrypt = new FileStream(inFile, FileMode.Open);
            fsCrypt.Read(salt, 0, salt.Length);

            RijndaelManaged AES = new RijndaelManaged();
            var key = new Rfc2898DeriveBytes(passwordBytes, salt, 50000);
            AES.Key = key.GetBytes(32); //zozmiar klucza 
            AES.IV = key.GetBytes(16); //rozmiar bloka
            AES.Padding = PaddingMode.PKCS7;

            //Tryb szyfrowania AES: ECB, CBC, CFB, OFB, CTR
            AES.Mode = CipherMode.CFB;


            string dirName = Path.GetDirectoryName(inFile);
            string fName = Path.GetFileNameWithoutExtension(inFile);
            string fExtension = Path.GetExtension(inFile);
            
            CryptoStream cs = new CryptoStream(fsCrypt, AES.CreateDecryptor(), CryptoStreamMode.Read);
            FileStream fsOut = new FileStream(dirName + "\\" + fName, FileMode.Create);

            int read;
            byte[] buffer = new byte[1048576];


            while ((read = cs.Read(buffer, 0, buffer.Length)) > 0)
                fsOut.Write(buffer, 0, read);

            cs.Close();

            File.Delete(@dirName + "\\" + fName + fExtension);
            fsOut.Close();
            fsCrypt.Close();
        
        }

    }
}
