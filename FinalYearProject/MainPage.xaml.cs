using FinalYearProject.Screens;
using FinalYearProject.ViewModels;
using System.Threading.Tasks;
using FinalYearProject.Database;
using FinalYearProject.Services;

namespace FinalYearProject
{
    public partial class MainPage : ContentPage
    {

        public MainPage(MainViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;

        }
    }

}
