namespace FinalYearProject.Screens.ContentViews.SignUpContentViews;

public partial class SignUpLocation : ContentView
{
	public SignUpLocation(SignUpViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}