namespace FinalYearProject.Screens.ContentViews.CalculationResults;

public partial class ShippingCalculationResults : ContentView
{
	public ShippingCalculationResults(ShippingViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}