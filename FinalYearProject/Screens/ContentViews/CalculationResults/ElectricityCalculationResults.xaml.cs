namespace FinalYearProject.Screens.ContentViews.CalculationResults;

public partial class ElectricityCalculationResults : ContentView
{
	public ElectricityCalculationResults(ElectricityViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}