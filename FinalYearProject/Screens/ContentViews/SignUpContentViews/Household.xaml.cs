namespace FinalYearProject.Screens.ContentViews.SignUpContentViews;

public partial class Household : ContentView
{
	public Household(SignUpViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}