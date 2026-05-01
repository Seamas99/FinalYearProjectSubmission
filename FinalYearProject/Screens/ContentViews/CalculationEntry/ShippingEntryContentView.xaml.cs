namespace FinalYearProject.Screens.ContentViews.CalculationEntry;

public partial class ShippingEntryContentView : ContentView
{
	public ShippingEntryContentView(ShippingViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}