namespace FinalYearProject.Screens.ContentViews.SignUpContentViews;

public partial class OrganisationContentView : ContentView
{
	public OrganisationContentView(SignUpViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}