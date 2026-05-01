using FinalYearProject.Interfaces;
using FinalYearProject.Services;
using Firebase.Auth;
using FinalYearProject.Database;

namespace FinalYearProject
{
    public partial class App : Application
    {
        private readonly ISettingsService _settingsService;
        private readonly FirebaseAuthClient _auth;
        public bool IsLoggedIn => _auth.User != null;
        public App(FirebaseAuthClient auth)
        {
            _auth = auth;
            _settingsService = new SettingsService();
            _settingsService.LoadSettings();
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }

        protected override void OnStart()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                MainPage = new AppShell();

                if (IsLoggedIn)
                {
                    Shell.Current.GoToAsync($"/{nameof(MainPage)}", true);
                }
                else
                {
                    Shell.Current.GoToAsync($"/{nameof(Screens.SignInView)}", true);
                }
                
            });

            base.OnStart();
        }
    }
}