using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GonvoProect.Modles;

namespace GonvoProect.Services
{
    internal class DataRepository
    {

        public static List< User > Data = new List<User>();

        public static void Add( User user)
        {
            if( !UserExists(user.Email) )
                Data.Add(user);
        }
        public static bool UserExists( string email )
        {
            return Data.Any(user => user.Email == email);
        }

        public static bool UserExists(string email, string password)
        {
            return Data.Any(user => user.Email == email && user.Password == password);
        }
    }
}
