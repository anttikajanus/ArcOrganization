namespace ArcOrganization.Hub.NewServices
{
    using System;
    using System.Threading.Tasks;
    using System.Windows;

    using ArcOrganization.Infrastructure.ViewModels;
    using ArcOrganization.Resources;
    using ArcOrganization.Services;

    using Caliburn.Micro;

    using ESRI.ArcGIS.Client.Portal;

    public class NewServicesViewModel : WebMapListViewModel 
    {
        private readonly PortalService _portalService;

        private readonly INavigationService _navigationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="NewServicesViewModel"/> class. 
        /// </summary>
        /// <param name="navigationService"></param>
        /// <param name="portalService"></param>
        public NewServicesViewModel(INavigationService navigationService, PortalService portalService)
        {
            _navigationService = navigationService;
            _portalService = portalService;

            DisplayName = TextResources.NewFeatureServicesTitle;
        }

        protected override void WebMapChanged(ArcGISPortalItem portalItem)
        {
            if (portalItem == null)
            {
                return;
            }

            // TODO and remove message box...
            MessageBox.Show("Not implemented");
        }

        /// <summary>
        /// Load Recent items asynchronously
        /// </summary>
        public override async Task LoadContentAsync()
        {           
            try
            {
                var results = await _portalService.GetNewestOrganizationFeatureServicesAsync();
                if (results == null)
                {
                    return;
                }

                Items = new BindableCollection<ArcGISPortalItem>(results);
                IsLoaded = true;
            }
            catch (Exception exception)
            {
                // TODO
            }
        }
      
    }
}
