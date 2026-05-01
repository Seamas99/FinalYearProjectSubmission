namespace FinalYearProject.Screens;

public partial class Progress : ContentPage
{
	public Progress(ProgressViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
    }
}