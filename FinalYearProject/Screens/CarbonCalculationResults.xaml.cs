namespace FinalYearProject.Screens;

public partial class CarbonCalculationResults : ContentPage
{
	public CarbonCalculationResults(CreateEstimateViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
    }
}