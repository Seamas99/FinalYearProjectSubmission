namespace FinalYearProject.Screens;

public partial class Profile : ContentPage
{
	public Profile(ProfileViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
    }
}