namespace FinalYearProject.Screens.ContentViews.CalculationResults;

public partial class FlightCalculationResults : ContentView
{
	public FlightCalculationResults(FlightViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}