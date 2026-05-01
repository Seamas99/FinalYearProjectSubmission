namespace FinalYearProject.Screens;

public partial class VehicleInformationScreen : ContentPage
{
	public VehicleInformationScreen(VehicleViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}