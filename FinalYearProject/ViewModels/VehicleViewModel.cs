using CommunityToolkit.Mvvm.Messaging;
using FinalYearProject.Database;
using FinalYearProject.Helper;
using FinalYearProject.Interfaces;
using FinalYearProject.Messages;
using FinalYearProject.Screens;
using FinalYearProject.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalYearProject.ViewModels
{
    public partial class VehicleViewModel : BaseViewModel
    {
        private readonly ICarbonService _carbonService;
        private readonly ISettingsService _settingsService;
        private static readonly DateTime currentDate = DateTime.UtcNow;

        [ObservableProperty]
        private bool isBusy = false;

        [ObservableProperty]
        private bool isContentVisible = true;

        public VehicleViewModel(ICarbonService carbonService, ISettingsService settingsService)
        {
            Title = "Vehicle Calculator";
            this._carbonService = carbonService;
            this._settingsService = settingsService;

            WeakReferenceMessenger.Default.Register<SettingsChangedMessage>(this, (recipient, message) =>
            {
                DistanceUnit = _settingsService.DistanceUnit;
                WeightUnit = _settingsService.WeightUnit;
            });

            _ = InitialiseAsync();
        }

        async Task InitialiseAsync()
        {
            try
            {
                IsBusy = true;
                IsContentVisible = false;

                await PopulateVehicleMakes();
                await PopulateVehicleModels();
                await LoadSavedVehicles();
                DistanceUnit = _settingsService.DistanceUnit;
                WeightUnit = _settingsService.WeightUnit;
            }
            finally
            {
                IsBusy = false;
                IsContentVisible = true;
            }
        }

        public ObservableCollection<Vehicle> Vehicles { get; } = new();
        public ObservableCollection<SavedVehicle> SavedVehicles { get; } = new();
        public ObservableCollection<VehicleMake> VehicleMakes { get; } = new();
        public ObservableCollection<FullVehicleMake> FullVehicleMakes { get; } = new ObservableCollection<FullVehicleMake>();
        public ObservableCollection<VehicleModel> VehicleModel { get; } = new();
        public ObservableCollection<FullVehicleModel> FullVehicleModelsList { get; } = new();

        //List of Vehicle Models belonging to selected Make
        public ObservableCollection<FullVehicleModel> SelectedMakeListOfVehicleModels { get; } = new();

        [ObservableProperty]
        string distanceUnit;

        [ObservableProperty]
        string weightUnit;

        [ObservableProperty]
        int distanceTravelled;

        [ObservableProperty]
        float carbonGenerated;
        partial void OnCarbonGeneratedChanged(float carbon)
        {
            Debug.WriteLine(CarbonGenerated.ToString());
        }

        [ObservableProperty]
        FullVehicleMake selectedMake;
        partial void OnSelectedMakeChanged(FullVehicleMake vehicleMake)
        {
            LoadVehicleModelsFromMake(vehicleMake.name);
        }

        [ObservableProperty]
        FullVehicleModel selectedModel;

        [ObservableProperty]
        bool isManualEntry = false;

        [ObservableProperty]
        bool isSavedVehiclesPopulated = false;

        [ObservableProperty]
        bool detailsEnteredManually = true;

        [ObservableProperty]
        SavedVehicle selectedVehicle;
        partial void OnSelectedVehicleChanged(SavedVehicle oldValue, SavedVehicle newValue)
        {
            IsManualEntry = newValue == null || newValue.id == string.Empty;

            if (!IsManualEntry)
            {
                SelectedMake = FullVehicleMakes.Where(vm => vm.name == newValue.vehicle_make).FirstOrDefault();
                SelectedModel = FullVehicleModelsList.Where(vm => vm.name == newValue.model_name).FirstOrDefault();
                DetailsEnteredManually = false;
            }
            else
            {
                DetailsEnteredManually = true;
            }
        }
        [RelayCommand]
        async Task CalculateFootprintAsync()
        {
            string ModelID = await FindVehicleID(SelectedModel.name);
            if (DetailsEnteredManually)
            {
                bool response = await Shell.Current.DisplayAlert(
                                                    "Save Vehicle?",
                                                    "Would you like to save this vehicle?",
                                                    "Save",
                                                    "Don't Save");
                if (response)
                {
                    string result = await Shell.Current.DisplayPromptAsync("Name Vehicle", "What would you like to name this vehicle?");

                    SavedVehicle savedVehicle = new SavedVehicle();
                    savedVehicle.name = result;
                    savedVehicle.year = SelectedModel.year;
                    savedVehicle.model_name = SelectedModel.name;
                    savedVehicle.vehicle_make = SelectedModel.vehicle_make;
                    savedVehicle.carbon_interface_id = SelectedModel.id;
                    savedVehicle.id = Guid.NewGuid().ToString();

                    ProfileHelper profileHelper = new ProfileHelper();
                    await profileHelper.InsertSavedVehicleToDatabase(savedVehicle);
                    LoadSavedVehicles();
                }
            }

            try
            {
                IsBusy = true;
                IsContentVisible = false;
                await GetVehicleAsync(ModelID, DistanceTravelled);

                if (WeightUnit == "g")
                {
                    CarbonGenerated = Vehicles.FirstOrDefault().data.attributes.carbon_g;
                }
                else if (WeightUnit == "lb")
                {
                    CarbonGenerated = Vehicles.FirstOrDefault().data.attributes.carbon_lb;

                }
                else if (WeightUnit == "kg")
                {
                    CarbonGenerated = Vehicles.FirstOrDefault().data.attributes.carbon_kg;

                }
                else if (WeightUnit == "mt")
                {
                    CarbonGenerated = Vehicles.FirstOrDefault().data.attributes.carbon_mt;

                }

                foreach (Vehicle v in Vehicles)
                {
                    Debug.WriteLine(v.data.attributes.carbon_g);
                }
                await Shell.Current.GoToAsync($"/{nameof(Screens.CarbonCalculationResults)}", true);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error!",
                    $"Unable to get Vehicle information", "OK");
                return;
            }
            finally
            {
                IsBusy = false;
                IsContentVisible = true;
            }
        }

        [RelayCommand]
        async Task TestFootprintAsync()
        {
            Vehicle vehicle = new();
            vehicle.data = new();
            vehicle.data.attributes = new();
            vehicle.data.id = "6108d711-be04-4dc4-93f9-43d969fd5273";
            vehicle.data.type = "estimate";
            vehicle.data.attributes.distance_value = 100.0F;
            vehicle.data.attributes.vehicle_make = "Toyota";
            vehicle.data.attributes.vehicle_model = "Corolla";
            vehicle.data.attributes.vehicle_year = 1993;
            vehicle.data.attributes.vehicle_model_id = "7268a9b7-17e8-4c8d-acca-57059252afe9";
            string distanceUnitMsg = "";
            if (DistanceUnit == "km")
            {
                distanceUnitMsg = "km";
            }
            else
            {
                distanceUnitMsg = "mi";
            }
            vehicle.data.attributes.distance_unit = distanceUnitMsg;
            vehicle.data.attributes.estimated_at = DateTime.Parse("2021-01-10T15:24:32.568Z");
            vehicle.data.attributes.carbon_g = 37029;
            vehicle.data.attributes.carbon_lb = 81.64F;
            vehicle.data.attributes.carbon_kg = 37.03F;
            vehicle.data.attributes.carbon_mt = 0.04F;
            if (WeightUnit == "g")
            {
                CarbonGenerated = vehicle.data.attributes.carbon_g;
            }
            else if (WeightUnit == "lb")
            {
                CarbonGenerated = vehicle.data.attributes.carbon_lb;

            }
            else if (WeightUnit == "kg")
            {
                CarbonGenerated = vehicle.data.attributes.carbon_kg;

            }
            else if (WeightUnit == "mt")
            {
                CarbonGenerated = vehicle.data.attributes.carbon_mt;

            }
            DistanceTravelled = 1000;
            await Shell.Current.GoToAsync($"/{nameof(Screens.CarbonCalculationResults)}", true);
        }

        async Task<string> FindVehicleID(string name)
        {
            string ModelID = "";
            foreach (FullVehicleModel v in SelectedMakeListOfVehicleModels)
            {
                if (v.name == name)
                {
                    ModelID = v.id;
                }
            }
            return ModelID;
        }

        async Task CacheVehicleModels(FullVehicleModel fullVehicleModel)
        {
            VehicleModelHelper vehicleModelHelper = new();
            if (!FullVehicleModelsList.Contains(fullVehicleModel))
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    FullVehicleModelsList.Add(fullVehicleModel);
                });

                await vehicleModelHelper.CreateNewVehicleModel(fullVehicleModel);
            }
        }

        async Task LoadVehicleModelsFromMake(string vehicleMake)
        {
            SelectedMakeListOfVehicleModels.Clear();

            var seenNames = new HashSet<string>();

            foreach (FullVehicleModel v in FullVehicleModelsList)
            {
                if (v.vehicle_make == vehicleMake && !seenNames.Contains(v.name))
                {
                    seenNames.Add(v.name);
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        SelectedMakeListOfVehicleModels.Add(v);
                    });
                }
            }
            if (SelectedMakeListOfVehicleModels.Count == 0)
            {
                string vehicleMakeID = "";
                foreach (FullVehicleMake v in FullVehicleMakes)
                {
                    if (v.name == vehicleMake)
                    {
                        vehicleMakeID = v.id;
                    }
                }

                List<VehicleModel> vehicleModelsList = await _carbonService.GetVehicleModels(vehicleMakeID).ConfigureAwait(false);


                foreach (VehicleModel v in vehicleModelsList)
                {
                    FullVehicleModel fullVehicleModel = VehicleModelHelper.ReturnFullVehicleModel(v);
                    CacheVehicleModels(fullVehicleModel);

                    if (!seenNames.Contains(fullVehicleModel.name))
                    {
                        seenNames.Add(fullVehicleModel.name);
                        await MainThread.InvokeOnMainThreadAsync(() =>
                        {
                            SelectedMakeListOfVehicleModels.Add(fullVehicleModel);
                        });
                    }

                }
            }
        }

        //this will only retrieve models from db already previously requested via api call for specific make
        //e.g. if user has selected ford make, but not volkswagen previously ford will be in db but volkswagen will not
        async Task PopulateVehicleModels()
        {
            List<FullVehicleModel> fullVehicleModels = (await VehicleModelHelper.LoadAllFullVehicleModels().ConfigureAwait(false)).ToList();

            foreach (FullVehicleModel v in fullVehicleModels)
            {
                //This if else should be unnecessary but want to avoid any possibility of double entries
                if (!FullVehicleModelsList.Contains(v))
                {
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        FullVehicleModelsList.Add(v);
                    });
                }
            }
        }

        async Task LoadSavedVehicles()
        {
            List<SavedVehicle> savedVehicles = new List<SavedVehicle>();
            ProfileHelper profileHelper = new ProfileHelper();
            savedVehicles = await ProfileHelper.LoadSavedVehicles();

            SavedVehicles.Clear();

            SavedVehicle manualEntryOption = new SavedVehicle
            {
                id = string.Empty,
                name = "Enter manually..."
            };
            SavedVehicles.Add(manualEntryOption);

            if (savedVehicles.Count > 0)
            {
                foreach (SavedVehicle vehicle in savedVehicles)
                {
                    SavedVehicles.Add(vehicle);
                }
                IsSavedVehiclesPopulated = true;
            }
            else
            {
                IsManualEntry = true;
            }
        }

        async Task PopulateVehicleMakes()
        {
            List<FullVehicleMake> fullVehicleMakes = (await VehicleMakeHelper.LoadAllFullVehicleMakes().ConfigureAwait(false)).ToList();

            if (fullVehicleMakes.Count == 138) //The number of vehicle makes shouldn't change, this will avoid unnecessary API calls every time wasting credits
            {
                //This if else should be unnecessary, fullVehicleMakes should always be either 0 or 138, but want to avoid any possibility of double entries
                if (FullVehicleMakes.Count == 0)
                {
                    foreach (FullVehicleMake v in fullVehicleMakes)
                    {
                        await MainThread.InvokeOnMainThreadAsync(() =>
                        {
                            FullVehicleMakes.Add(v);
                        });
                    }
                }
                else
                {
                    foreach (FullVehicleMake v in fullVehicleMakes)
                    {
                        await MainThread.InvokeOnMainThreadAsync(() =>
                        {
                            FullVehicleMakes.Add(v);
                        });
                    }
                }

            }
            else //else the db hasn't got 138 makes and need to retrieve them via api
            {
                List<VehicleMake> vehicleMakes = await _carbonService.GetVehicleMakes().ConfigureAwait(false);

                VehicleMakeHelper vehicleMakeHelper = new();
                foreach (VehicleMake v in vehicleMakes)
                {
                    FullVehicleMake fullVehicleMake = VehicleMakeHelper.ReturnFullVehicleMake(v);

                    //again this if should be unnecessary, but this avoids having double entries
                    if (!FullVehicleMakes.Contains(fullVehicleMake))
                    {
                        await MainThread.InvokeOnMainThreadAsync(() =>
                        {
                            FullVehicleMakes.Add(fullVehicleMake);
                        });
                        await vehicleMakeHelper.CreateNewVehicleMake(fullVehicleMake);
                    }
                }
            }

            Debug.WriteLine($"Makes loaded: {VehicleMakes.Count}");

        }

        async Task GetVehicleAsync(string modelID, int distanceValue)
        {
            try
            {
                IsBusy = true;
                IsContentVisible = false;

                List<Vehicle> vehicles = new();

                string distanceUnitMsg = "";
                if (DistanceUnit.ToLower() == "km")
                {
                    distanceUnitMsg = "km";
                }
                else
                {
                    distanceUnitMsg = "mi";
                }

                vehicles = await _carbonService.GetVehicles(modelID, distanceUnitMsg, distanceValue);

                if (Vehicles.Count != 0)
                    Vehicles.Clear();

                foreach (var vehicle in vehicles)
                    Vehicles.Add(vehicle);
                vehicles.Clear(); //for some reason vehicles still had values from previous requests despite leaving scope...
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                await Shell.Current.DisplayAlert("Error!",
                    $"Unable to get Vehicle information", "OK");
            }
            finally
            {
                IsBusy = false;
                IsContentVisible = true;
            }
        }

    }
}
