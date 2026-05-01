namespace FinalYearProject.Screens;

public partial class AlertPage : ContentPage
{
	public AlertPage(AlertViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}