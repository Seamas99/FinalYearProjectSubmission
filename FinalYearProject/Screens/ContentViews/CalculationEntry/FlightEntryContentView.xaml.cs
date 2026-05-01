namespace FinalYearProject.Screens.ContentViews.CalculationEntry;

public partial class FlightEntryContentView : ContentView
{
	public FlightEntryContentView(FlightViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}