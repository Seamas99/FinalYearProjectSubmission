using CommunityToolkit;
using CommunityToolkit.Maui;
using FinalYearProject.Database;
using FinalYearProject.Interfaces;
using FinalYearProject.Screens;
using FinalYearProject.Screens.ContentViews;
using FinalYearProject.Screens.ContentViews.CalculationEntry;
using FinalYearProject.Screens.ContentViews.CalculationResults;
using FinalYearProject.Screens.ContentViews.SignUpContentViews;
using FinalYearProject.Services;
using FinalYearProject.ViewModels;
using Firebase.Auth;
using Firebase.Auth.Providers;
using Firebase.Auth.Repository;
using Microcharts.Maui;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Toolkit;
using Syncfusion.Maui.Toolkit.Hosting;

namespace FinalYearProject
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMicrocharts()
                .UseMauiCommunityToolkit()
                .ConfigureSyncfusionToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif
            builder.Services.AddSingleton(new FirebaseAuthClient(new FirebaseAuthConfig()
            {
                //API key in code shouldn't matter as Firebase API keys are only used for identification of application
                ApiKey = "AIzaSyCaBu9XzItQQX_jAZ8FnL_FToKU5xJ1lH0",
                AuthDomain = "final-year-project-484713.firebaseapp.com",
                Providers = new Firebase.Auth.Providers.FirebaseAuthProvider[]
                {
                    new EmailProvider()
                },
                UserRepository = new FileUserRepository("CarbonCompass")
            }));

            builder.Services.AddSingleton<ICarbonService, CarbonService>();
            builder.Services.AddSingleton<IProfileService, ProfileService>();
            builder.Services.AddSingleton<ISettingsService, SettingsService>();
            builder.Services.AddHttpClient<ICarbonService, CarbonService>();
            builder.Services.AddSingleton<IMapBoxService, MapBoxService>();

            builder.Services.AddSingleton<MainPage>();
            builder.Services.AddSingleton<Screens.Profile>();
            builder.Services.AddSingleton<Screens.AlertPage>();

            builder.Services.AddTransient<MainViewModel>();
            builder.Services.AddSingleton<CreateEstimateViewModel>();
            builder.Services.AddSingleton<ProfileViewModel>();
            builder.Services.AddSingleton<AlertViewModel>();
            builder.Services.AddSingleton<VehicleViewModel>();
            builder.Services.AddSingleton<SettingsViewModel>();
            builder.Services.AddSingleton<SignUpViewModel>();
            builder.Services.AddSingleton<SignInViewModel>();
            builder.Services.AddSingleton<LeaderboardViewModel>();
            builder.Services.AddSingleton<ProgressViewModel>();
            builder.Services.AddSingleton<ChallengeViewModel>();

            builder.Services.AddTransient<VehicleInformationScreen>();
            builder.Services.AddTransient<CalculationSelectionScreen>();
            builder.Services.AddTransient<SettingsScreen>();
            builder.Services.AddTransient<CarbonCalculationResults>();
            builder.Services.AddTransient<CreateEstimatePage>();
            builder.Services.AddTransient<SignUpView>();
            builder.Services.AddTransient<SignInView>();
            builder.Services.AddTransient<CreateSavedVehicle>();

            builder.Services.AddTransient<VehicleEntryContentView>();
            builder.Services.AddTransient<FlightEntryContentView>();
            builder.Services.AddTransient<ShippingEntryContentView>();
            builder.Services.AddTransient<ElectricityEntryContentView>();
            builder.Services.AddTransient<SignUpPreferences>();
            builder.Services.AddTransient<Personal>();
            builder.Services.AddTransient<OrganisationContentView>();
            builder.Services.AddTransient<SignUpLocation>();
            builder.Services.AddTransient<Household>();
            builder.Services.AddTransient<BasicInfo>();

            builder.Services.AddTransient<VehicleCalculationResults>();
            builder.Services.AddTransient<FlightCalculationResults>();
            builder.Services.AddTransient<ShippingCalculationResults>();
            builder.Services.AddTransient<ElectricityCalculationResults>();

            return builder.Build();
        }
    }
}
