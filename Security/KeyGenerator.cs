using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalDatabase_Client.Security
{
    public static class KeyGenerator
    {
        private static System.Random r = new System.Random();
        static string Generate()
        {
            int key_len = 32; //klucz o długości 256 bitów

            //Alfabet przygotowany do stworzenia klucza:
            char[] b_letters = new char[] { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'Y', 'X', 'Z' };
            char[] s_letters = new char[] { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'y', 'x', 'z' };
            char[] numbers = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
            char[] symbols = new char[] { '!', '@', '#', '$', '%', '&', '*', '=', ']', '[', '/', '+', '~' };


            string key = string.Empty;

            for (int i = 0; i < key_len; ++i)
            {
                int random = r.Next(4);
                if (random == 0)
                {
                    int location = r.Next(26);
                    key += b_letters[location];
                }
                else if (random == 1)
                {
                    int location = r.Next(10);
                    key += numbers[location];
                }
                else if (random == 2)
                {
                    int location = r.Next(26);
                    key += s_letters[location];
                }
                else
                {
                    int location = r.Next(13);
                    key += symbols[location];
                }
            }
            return key;
        }
    }
}
