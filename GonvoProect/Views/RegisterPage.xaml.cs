using Microsoft.Maui.ApplicationModel.Communication;
using System.Text.RegularExpressions;
using GonvoProect.Modles;
using GonvoProect.Services;
using GonvoProect.ViewModel;

namespace GonvoProect.Views;

public partial class RegisterPage : ContentPage
{
	public RegisterPage()
	{
		InitializeComponent();

        BindingContext = new RegisterViewModel();
    }
}