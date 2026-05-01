namespace FinalYearProject.Screens;

public partial class CalculationSelectionScreen : ContentPage
{
	public CalculationSelectionScreen(CreateEstimateViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
    }
}