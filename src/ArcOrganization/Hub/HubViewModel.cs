namespace ArcOrganization.Hub
{
    using System;
    using System.Threading.Tasks;

    using ArcOrganization.Hub.Account;
    using ArcOrganization.Hub.NewMaps;
    using ArcOrganization.Hub.NewServices;
    using ArcOrganization.Infrastructure.ViewModels;
    using ArcOrganization.Services;

    using Caliburn.Micro;

    public class HubViewModel : ConductorViewModel<SubScreenViewModel>
    {
        private readonly PortalService _portalService;

        private readonly INavigationService _navigationService;

        private DateTime _contentLoaded;

        public HubViewModel(
            NewMapsViewModel newMapsViewModel,
            NewServicesViewModel newServicesViewModel,
            AccountViewModel accountViewModel,
            PortalService portalService,
            INavigationService navigationService)
        {
            _portalService = portalService;
            _navigationService = navigationService;

            Items.Add(newMapsViewModel);
            Items.Add(newServicesViewModel);
            Items.Add(accountViewModel);
        }

        public async void RefreshActiveItemAsync()
        {
            try
            {
                await LoadAllContentAsync();
                _contentLoaded = DateTime.Now;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void NavigateToSearch()
        {
            // TODO
        }
    }
}