namespace FinalYearProject.Screens.ContentViews.CalculationResults;

public partial class VehicleCalculationResults : ContentView
{
	public VehicleCalculationResults(VehicleViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}