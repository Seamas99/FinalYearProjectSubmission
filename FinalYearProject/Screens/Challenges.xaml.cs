namespace FinalYearProject.Screens;

public partial class Challenges : ContentPage
{
	public Challenges(ChallengeViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}