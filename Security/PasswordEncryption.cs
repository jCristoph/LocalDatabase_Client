using System;
using System.Text;
using System.Security.Cryptography;

namespace LocalDatabase_Client.Security
{
    public static class PasswordEncryption
    {
        //password encryption method SHA256
        public static string encryption256(string password)
        {
            Byte[] passBytes = Encoding.UTF8.GetBytes(password);
            Byte[] hashBytes = new SHA256CryptoServiceProvider().ComputeHash(passBytes);
            return BitConverter.ToString(hashBytes);
        }

    }
}
