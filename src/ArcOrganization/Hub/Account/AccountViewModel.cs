namespace ArcOrganization.Hub.Account
{
    using System.Threading.Tasks;

    using ArcOrganization.Infrastructure.ViewModels;
    using ArcOrganization.Resources;
    using ArcOrganization.Services;

    using Caliburn.Micro;

    using ESRI.ArcGIS.Client.Portal;

    public class AccountViewModel : SubScreenViewModel
    {
        private readonly INavigationService _navigationService;

        private readonly PortalService _portalService;

        private ArcGISPortalUser _userInformation;

        public AccountViewModel(INavigationService navigationService, PortalService portalService)
        {
            _navigationService = navigationService;
            _portalService = portalService;

            DisplayName = TextResources.AccountTitle;
        }

        /// <summary>
        /// Gets the users information
        /// </summary>
        public ArcGISPortalUser UserInformation
        {
            get
            {
                return _userInformation;
            }

            private set
            {
                if (Equals(value, _userInformation))
                {
                    return;
                }

                _userInformation = value;
                NotifyOfPropertyChange(() => UserInformation);
            }
        }

        public override async Task LoadContentAsync()
        {
            UserInformation = await _portalService.GetUserInformationAsync();
            base.LoadContentAsync();
        }

    }
}