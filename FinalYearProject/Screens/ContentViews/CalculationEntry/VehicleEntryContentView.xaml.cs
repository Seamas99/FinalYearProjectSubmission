namespace FinalYearProject.Screens.ContentViews.CalculationEntry;

public partial class VehicleEntryContentView : ContentView
{
	public VehicleEntryContentView(VehicleViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}