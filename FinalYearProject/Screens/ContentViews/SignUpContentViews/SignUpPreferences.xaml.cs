namespace FinalYearProject.Screens.ContentViews.SignUpContentViews;

public partial class SignUpPreferences : ContentView
{
	public SignUpPreferences(SignUpViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}