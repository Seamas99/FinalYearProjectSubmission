namespace FinalYearProject.Screens;

public partial class CreateEstimatePage : ContentPage
{
	public CreateEstimatePage(CreateEstimateViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}