using GonvoProect.ViewModel;
using GonvoProect.ViewModels;

namespace GonvoProect.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();

        BindingContext = new LoginViewModel();
    }
}