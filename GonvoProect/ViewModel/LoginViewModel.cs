using GonvoProect.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GonvoProect.Views;
using System.Text.RegularExpressions;
using GonvoProect.Services;


namespace GonvoProect.ViewModel
{
    internal class LoginViewModel : ViewModelBase
    {

        private string email = "";
        public string Email
        {
            get => email;
            set => SetProperty(ref email, value);
        }

        private string errEmail = "";
        public string ErrEmail
        {
            get => errEmail;
            set => SetProperty(ref errEmail, value);
        }


        private string password = "";
        public string Password
        {
            get => password;
            set => SetProperty(ref password, value);
        }

        private string errPassword = "";
        public string ErrPassword
        {
            get => errPassword;
            set => SetProperty(ref errPassword, value);
        }


        public Command SignUpCommand { get; }
        public Command RegisterCommand { get; }
        public LoginViewModel()
        {
            SignUpCommand = new Command(SignUp);
            RegisterCommand = new Command(Register);
        }

        private async void Register()
        {
            await Shell.Current.GoToAsync($"///{nameof(RegisterPage)}");
        }
        private async void SignUp()
        {
            bool vaild = true;

            if(!Regex.IsMatch(Email.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                ErrEmail = "Invalid email format";
                vaild = false;
            }
            else ErrEmail = "";


            if (Password == "" || Password == null)
            {
                ErrPassword = "Password cannot be empty";
                vaild = false;
            }
            else ErrPassword = "";


            if (!vaild) return;
            if (!DataRepository.UserExists(Email, Password))
            {
                ErrPassword = "Password or Email is incorrect";
                ErrEmail = "Password or Email is incorrect";
                vaild = false;
            }

            if (!vaild) return;

                await Shell.Current.GoToAsync($"///{nameof(MainPage)}");
        }

    }
}
