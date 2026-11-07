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



        private string password = "";
        public string Password
        {
            get => password;
            set => SetProperty(ref password, value);
        }

        private string error = "";
        public string Error
        {
            get => error;
            set => SetProperty(ref error, value);
        }

        private bool errorvis = false;
        public bool ErrorVis
        {
            get => errorvis;
            set => SetProperty(ref errorvis, value);
        }

        private bool isPasswordHidden = true;
        public bool IsPasswordHidden
        {
            get => isPasswordHidden;
            set => SetProperty(ref isPasswordHidden, value);
        }

        public string passwordEyeIcona = "eye_off.png";
        public string PasswordEyeIcona
        {
            get => IsPasswordHidden ? "eye_off.png" : "eye.png";
        }


        public Command EyeClickCommand { get; }
        public Command LoginCommand { get; }
        public Command RegisterCommand { get; }
        public LoginViewModel()
        {
            EyeClickCommand = new Command(EyeClick);
            LoginCommand = new Command(Login);
            RegisterCommand = new Command(Register);
        }

        public async void EyeClick()
        {
            IsPasswordHidden ^= true;
            OnPropertyChanged(nameof(PasswordEyeIcona));
        }


        private async void Register()
        {
            await Shell.Current.GoToAsync($"///{nameof(RegisterPage)}");
        }
        private async void Login()
        {

            if( DataRepository.UserExists(Email, Password) )
            {
                await Shell.Current.GoToAsync($"///{nameof(MainPage)}");
                return;
            }

            ErrorVis = true;
            Error = "The Email or Password is not correct";

            
        }

    }
}
