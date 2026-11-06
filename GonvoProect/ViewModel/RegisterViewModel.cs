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



namespace GonvoProect.ViewModel
{
    internal class RegisterViewModel : ViewModelBase
    {

        private string name = "";
        public string Name
        {
            get => name;
            set => SetProperty(ref name, value);
        }


        private string email ="";
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

        private string errName = "";
        public string ErrName
        {
            get => errName;
            set => SetProperty(ref errName, value);
        }

        private string errEmail = "";
        public string ErrEmail
        {
            get => errEmail;
            set => SetProperty(ref errEmail, value);
        }


        private string errPassword = "";
        public string ErrPassword
        {
            get => errPassword;
            set => SetProperty(ref errPassword, value);
        }


        private DateTime birthDate;
        public DateTime BirthDate
        {
            get => birthDate;
            set => SetProperty(ref birthDate, value);
        }

        private string errDate;
        public string ErrDate
        {
            get => errDate;
            set => SetProperty(ref errDate, value);
        }

        public Command RegisterCommand { get; }
        public RegisterViewModel()
        {
            RegisterCommand = new Command(Register);
        }

        private string successReg;
        public string SuccessReg
        {
            get => successReg;
            set => SetProperty(ref successReg, value);
        }

        private async void Register()
        {
            bool vaild = true;
            if (Name == "" || Name == null)
            {
                ErrName = "Name cannot be empty";
                vaild = false;
            } else ErrName = "";


            if (Email == "" || Email == null)
            {
                ErrEmail = "Email cannot be empty";
            }

            else if (!Regex.IsMatch(Email.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                ErrEmail = "Invalid email format";
                vaild = false;
            }
            else if (DataRepository.UserExists(Email))
            {
                ErrEmail = "this email alrady have been used";
                vaild = false;
            }
            else ErrEmail = "";

            if (Password == "" || Password == null)
            {
                ErrPassword = "Password cannot be empty";
                vaild = false;
            }
            else if (Password.Length < 8)
            {
                ErrPassword = "The Passowrd must be al least 8 symbols";
                vaild = false;
            }
            else ErrPassword = "";

            if (BirthDate >= DateTime.Today.AddYears(-5))
            {
                ErrDate = "You must be older than 5 years old";
                vaild = false;
            }
            else ErrDate = "";





            if (!vaild) return;

            User user = new User
            {
                Name = Name,
                Email = Email,
                Password = Password,
                BirthDate = BirthDate,
                MaxScore = 0
            };


            DataRepository.Add(user);

            SuccessReg = "Let's gooooo";
            await Shell.Current.GoToAsync($"///{nameof(LoginPage)}");
        }
    }
}
