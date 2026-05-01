using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalYearProject.ViewModels
{
    public partial class BaseViewModel: ObservableObject
    {
        [ObservableProperty]
        string title;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotBusy))]
        bool isBusy;

        public bool IsNotBusy => !IsBusy;

        public virtual Task<bool> CanNavigateAwayAsync()
        {
            //Default: allow navigation
            return Task.FromResult(true);
        }

        public interface IConfirmNavigation
        {
            Task<bool> CanNavigateAwayAsync();
        }

    }
}
