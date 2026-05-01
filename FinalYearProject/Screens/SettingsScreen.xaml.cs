namespace FinalYearProject.Screens;

public partial class SettingsScreen : ContentPage
{
	public SettingsScreen(SettingsViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}