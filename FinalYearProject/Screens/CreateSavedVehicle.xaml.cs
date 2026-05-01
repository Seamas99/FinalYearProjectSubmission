namespace FinalYearProject.Screens;

public partial class CreateSavedVehicle : ContentPage
{
	public CreateSavedVehicle(ProfileViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}