using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GonvoProect.Modles;

namespace GonvoProect.Services
{
    internal class DataRepository
    {

        public static ObservableCollection< User > Data = new ObservableCollection<User>();
        

        public static void Add( User user )
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
            if (email == null || email == "") return false;
            if (password == null || password == "") return false;
            return Data.Any(user => user.Email == email && user.Password == password);
        }
    }
}
