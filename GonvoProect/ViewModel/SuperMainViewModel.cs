using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GonvoProect.ViewModels;
using System.Text.RegularExpressions;
using GonvoProect.Services;
using GonvoProect.Views;
using GonvoProect.Modles;
using System.Collections.ObjectModel;

namespace GonvoProect.ViewModel
{
    internal class SuperMainViewModel : ViewModelBase
    {
        public ObservableCollection<User> Users { get; set; }

        public SuperMainViewModel()
        {
            Users = DataRepository.Data;
        }

    }
}
