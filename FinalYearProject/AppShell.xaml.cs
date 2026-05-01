using static FinalYearProject.ViewModels.BaseViewModel;
namespace FinalYearProject
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute("Profile", typeof(Screens.Profile));
            Routing.RegisterRoute("AlertPage", typeof(Screens.AlertPage));
            Routing.RegisterRoute("CalculationSelectionScreen", typeof(Screens.CalculationSelectionScreen));
            Routing.RegisterRoute("VehicleInformationScreen", typeof(Screens.VehicleInformationScreen));
            Routing.RegisterRoute("SettingsScreen", typeof(Screens.SettingsScreen));
            Routing.RegisterRoute("CarbonCalculationResults", typeof(Screens.CarbonCalculationResults));
            Routing.RegisterRoute("CreateEstimatePage", typeof(Screens.CreateEstimatePage));
            Routing.RegisterRoute("SignUpView", typeof(Screens.SignUpView));
            Routing.RegisterRoute("SignInView", typeof(Screens.SignInView));
            Routing.RegisterRoute("CreateSavedVehicle", typeof(Screens.CreateSavedVehicle));
        }

        bool _isConfirmingNavigation = false;

        protected override async void OnNavigating(ShellNavigatingEventArgs args)
        {
            base.OnNavigating(args);

            // Prevent recursion
            if (_isConfirmingNavigation)
                return;

            if (Shell.Current?.CurrentPage?.BindingContext is not BaseViewModel vm)
                return;

            args.Cancel(); // pause navigation

            _isConfirmingNavigation = true;

            bool canLeave = await vm.CanNavigateAwayAsync();

            if (canLeave)
                await Shell.Current.GoToAsync(args.Target.Location);

            _isConfirmingNavigation = false;
        }



    }
}
