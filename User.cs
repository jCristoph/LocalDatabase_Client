using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalDatabase_Client
{
    public class User
    {
        public string surname { set; get; }
        public string name { set; get; }
        public string token { set; get; }

        public User(string surname, string name, string token)
        {
            this.surname = surname;
            this.name = name;
            this.token = token;
        }

        public override string ToString()
        {
            return surname + " " + name;
        }
    }
}
