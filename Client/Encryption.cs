using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;


namespace LocalDatabase_Client.Client
{
    public static class Encryption
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
