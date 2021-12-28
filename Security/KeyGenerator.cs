
namespace LocalDatabase_Client.Security
{
    public static class KeyGenerator
    {
        private static System.Random r = new System.Random();
        public static string Generate()
        {
            int keyLength = 32; //klucz o długości 256 bitów

            //Alfabet przygotowany do stworzenia klucza:
            char[] uppercaseLetters = new char[] { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'Y', 'X', 'Z' };
            char[] lowercaseLetters = new char[] { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'y', 'x', 'z' };
            char[] numbers = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
            char[] symbols = new char[] { '!', '@', '#', '$', '%', '&', '*', '=', ']', '[', '/', '+', '~' };

            string shuffleResult = ShuffleKey(keyLength, uppercaseLetters, lowercaseLetters, numbers, symbols);
            return shuffleResult;
        }

        private static string ShuffleKey(int keyLength, char[] uppercaseLetters, char[] lowercaseLetters, char[] numbers, char[] symbols )
        {
            string key = string.Empty;

            for (int i = 0; i < keyLength; ++i)
            {
                int random = r.Next(4);
                switch (random)
                {
                    case 0:
                        {
                            int location = r.Next(26);
                            key += uppercaseLetters[location];
                            break;
                        }
                    case 1:
                        {
                            int location = r.Next(10);
                            key += numbers[location];
                            break;
                        }
                    case 2:
                        {
                            int location = r.Next(26);
                            key += lowercaseLetters[location];
                            break;
                        }
                    default:
                        {
                            int location = r.Next(13);
                            key += symbols[location];
                            break;
                        }
                }

            }
            return key;
        }
    }
}