using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GonvoProect.Modles;
using Microsoft.Maui.ApplicationModel.Communication;
using GonvoProect.ViewModels;
using System.Text.RegularExpressions;
using GonvoProect.Services;
using GonvoProect.Views;
using Microsoft.Win32;



namespace GonvoProect.ViewModel
{
    internal class RegisterViewModel : ViewModelBase
    {
        private string username;
        public string Username
        {
            get => username;
            set => SetProperty(ref username, value);
        }

        private string errusername;
        public string ErrUsername
        {
            get => errusername;
            set => SetProperty(ref errusername, value);
        }

        private bool errusernamevis = false;
        public bool ErrUsernameVis
        {
            get => errusernamevis;
            set => SetProperty(ref errusernamevis, value);
        }

        private string email;
        public string Email
        {
            get => email;
            set => SetProperty(ref email, value);
        }

        private string erremail;
        public string ErrEmail
        {
            get => erremail;
            set => SetProperty(ref erremail, value);
        }

        private bool erremailvis = false;
        public bool ErrEmailVis
        {
            get => erremailvis;
            set => SetProperty(ref erremailvis, value);
        }

        private string password;
        public string Password
        {
            get => password;
            set => SetProperty(ref password, value);
        }

        private string errpassword;
        public string ErrPassword
        {
            get => errpassword;
            set => SetProperty(ref errpassword, value);
        }

        private bool errpasswordvis = false;
        public bool ErrPasswordVis
        {
            get => errpasswordvis;
            set => SetProperty(ref errpasswordvis, value);
        }

        private string confirm;
        public string Confirm
        {
            get => confirm;
            set => SetProperty(ref confirm, value);
        }

        private string errconfirm;
        public string ErrConfirm
        {
            get => errconfirm;
            set => SetProperty(ref errconfirm, value);
        }

        private bool errconfirmvis = false;
        public bool ErrConfirmVis
        {
            get => errconfirmvis;
            set => SetProperty(ref errconfirmvis, value);
        }

        private DateTime birthDate = DateTime.Today;
        public DateTime BirthDate
        {
            get => birthDate;
            set => SetProperty(ref birthDate, value);
        }

        private string errbirthday;
        public string ErrBirthday
        {
            get => errbirthday;
            set => SetProperty(ref errbirthday, value);
        }

        private bool errbirthdayvis = false;
        public bool ErrBirthdayVis
        {
            get => errbirthdayvis;
            set => SetProperty(ref errbirthdayvis, value);
        }

        private DateTime maxBirthDate = DateTime.Today;
        public DateTime MaxBirthDate
        {
            get => maxBirthDate;
            set => SetProperty(ref maxBirthDate, value);
        }

        private bool isPasswordHidden = true;

        public bool IsPasswordHidden
        {
            get => isPasswordHidden;
            set
            {
                SetProperty(ref isPasswordHidden, value);
                OnPropertyChanged(nameof(PasswordEyeIcona));
            }
        }

        public string PasswordEyeIcona =>
            IsPasswordHidden ? "eye_off.png" : "eye.png" ;



        public RegisterViewModel()
        {
            LoginCommand = new Command(Login);
            RegisterCommand = new Command(Register);
            EyeClickCommand = new Command(EyeClick);
        }

        public Command RegisterCommand { get; }
        public Command LoginCommand { get; }
        public Command EyeClickCommand { get; }

        public async void EyeClick()
        {
            IsPasswordHidden ^= true;
        }
        public async void Login()
        {
            await Shell.Current.GoToAsync($"///{nameof(LoginPage)}");
        }
        private async void Register()
        {
            bool vaild = true;

            if ( Username == null || Username == "")
            {
                ErrUsername = "Username cannot be empty";
                ErrUsernameVis = true;
                vaild = false;
            }
            else ErrUsernameVis = false;

            if ( Email == null || Email == "" )
            {
                ErrEmail = "Email cannot be empty";
                ErrEmailVis = true;
                vaild = false;

            }

            else if (!Regex.IsMatch(Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                ErrEmail = "Invalid email address";
                ErrEmailVis = true;
                vaild = false;

            }
            else ErrEmailVis = false;

            if (Password == null || Password.Length < 8)
            {
                ErrPassword = "Password must be at least 8 symbols";
                ErrPasswordVis = true;
                vaild = false;
            }
            else ErrPasswordVis = false;

            if ( Confirm != Password)
            {
                ErrConfirm = "you are lox";
                ErrConfirmVis = true;
                vaild = false;
            }
            else ErrConfirmVis = false;


            if (BirthDate >= DateTime.Today.AddYears(-5))
            {
                ErrBirthday = "Sorry, you are too young hahaha";
                ErrBirthdayVis = true;
                vaild = false;
            }
            else ErrBirthdayVis = false;

            if (!vaild) return;


            User user = new User()
            {
                Username = Username,
                Password = Password,
                Email = Email,
                BirthDate = BirthDate
            };

            DataRepository.Add( user );

            await Shell.Current.GoToAsync($"///{nameof(LoginPage)}");
        }

    }
}