namespace FinalYearProject.Screens.ContentViews.CalculationEntry;

public partial class ElectricityEntryContentView : ContentView
{
	public ElectricityEntryContentView(ElectricityViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}