namespace FinalYearProject.Screens.ContentViews.CalculationEntry;

public partial class FlightLegEntryContentView : ContentView
{
	public FlightLegEntryContentView(FlightViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}