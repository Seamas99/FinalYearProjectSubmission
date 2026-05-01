namespace FinalYearProject.Screens;

public partial class SignInView : ContentPage
{
	public SignInView(SignInViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}