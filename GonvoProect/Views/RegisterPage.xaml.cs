using Microsoft.Maui.ApplicationModel.Communication;
using System.Text.RegularExpressions;
using GonvoProect.Modles;
using GonvoProect.Services;

namespace GonvoProect.Views;

public partial class RegisterPage : ContentPage
{
	public RegisterPage()
	{
		InitializeComponent();
	}

	private async void OnSubmitClicked(object sender, EventArgs e)
	{
		const int MIN_AGE = 16;


        string Name = NameEntry.Text;
		string Email = EmailEntry.Text;
		string Password = PasswordEntry.Text;
		DateTime Date = BirthdayPicker.Date;

		bool Vaild = true;

		if (Name == "" || Name == null)
		{
			errName.Text = "The name can not be empty";
			Vaild = false;
		}
		else errName.Text = "";

		string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

		if (Email == "" || Email == null)
		{
			errEmail.Text = "Email can not be empty";
			Vaild = false;
		}

		else if (!Regex.IsMatch(Email, pattern))
		{
			errEmail.Text = "Invalid email format";
			Vaild = false;
		}
		else errEmail.Text = "";

		if (Password == null || Password.Length < 8)
		{
			errPassword.Text = "Passoword must contains at least 8 symbols";
			Vaild = false;
		}
		else errPassword.Text = "";


		if (Date > DateTime.Today.AddYears(-MIN_AGE))
		{
			errDate.Text = "You must be at least 16 years old";
			Vaild = false;
		}
		else errDate.Text = "";


		if (Vaild)
		{
			User user = new User
			{
				Name = NameEntry.Text,
				Email = EmailEntry.Text,
				Password = PasswordEntry.Text,
				BirthDate = BirthdayPicker.Date,
				MaxScore = 0,
			};

			if (DataRepository.UserExists(Email))
			{
				errEmail.Text = "This email is exist";
				return;
			}
			else errEmail.Text = "";

			DataRepository.Add(user);

			EmailEntry.Text = "";
			PasswordEntry.Text = "";
			NameEntry.Text = "";
			successRegister.Text = "Registration successful!";

        }
    }
}