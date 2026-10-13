using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GonvoProect.Modles
{
    internal class User
    {

        public string Name { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }

        public DateTime BirthDate { get; set; }
        public int MaxScore { get; set; }        

    }
}
