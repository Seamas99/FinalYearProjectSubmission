namespace FinalYearProject.Screens;

public partial class Leaderboard : ContentPage
{
    private LeaderboardViewModel _vm;
    public Leaderboard(LeaderboardViewModel vm)
	{
		InitializeComponent();
        BindingContext = _vm = vm;

    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadLeaderboards();
    }
}