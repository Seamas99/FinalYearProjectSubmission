namespace FinalYearProject.Screens.ContentViews.SignUpContentViews;

public partial class BasicInfo : ContentView
{
	public BasicInfo(SignUpViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}