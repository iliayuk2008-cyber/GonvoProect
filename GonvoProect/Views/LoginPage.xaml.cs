using GonvoProect.Services;

namespace GonvoProect.Views;

public partial class LoginPage : ContentPage
{
	public LoginPage()
	{
		InitializeComponent();
	}

    private async void OnSubmitClicked(object sender, EventArgs e)
	{
        //if( successLogin.Text != null ) successLogin.Text = "";


        string Email = EmailEntry.Text;
		string Password = PasswordEntry.Text;

		if (Email == null || Password == null || Email == "" || Password == "")
		{
			errLogin.Text = "The Email or Password can not be empty";
			return;
		}
		else errLogin.Text = "";

		if (!DataRepository.UserExists(Email, Password))
		{
			errLogin.Text = "You didn't Resister";
		}

		else
		{
			errLogin.Text = "";
			EmailEntry.Text = "";
			PasswordEntry.Text = "";
			successLogin.Text = "You are registered";
        }
	}

	private async void OnRegisterClicked( object sender, EventArgs e)
	{
        await Shell.Current.GoToAsync($"///{nameof(RegisterPage)}");
    }
}