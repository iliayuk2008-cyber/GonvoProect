using GonvoProect.ViewModel;

namespace GonvoProect.Views;

public partial class SuperMainPage : ContentPage
{
	public SuperMainPage()
	{
		InitializeComponent();

        BindingContext = new SuperMainViewModel();
    }
}