namespace FinalYearProject.Screens.ContentViews.SignUpContentViews;

public partial class Personal : ContentView
{
	public Personal(SignUpViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}