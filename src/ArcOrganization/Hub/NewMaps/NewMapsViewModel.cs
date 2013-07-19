namespace ArcOrganization.Hub.NewMaps
{
    using System;
    using System.Threading.Tasks;

    using ArcOrganization.Infrastructure.ViewModels;
    using ArcOrganization.Resources;
    using ArcOrganization.Services;
    using ArcOrganization.WepMap;

    using Caliburn.Micro;

    using ESRI.ArcGIS.Client.Portal;

    public class NewMapsViewModel : WebMapListViewModel
    {
        private readonly PortalService _portalService;

        private readonly INavigationService _navigationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="NewMapsViewModel"/> class. 
        /// </summary>
        /// <param name="navigationService"></param>
        /// <param name="portalService"></param>
        public NewMapsViewModel(INavigationService navigationService, PortalService portalService)
        {
            _navigationService = navigationService;
            _portalService = portalService;

            DisplayName = TextResources.NewMapsTitle;
        }

        protected override void WebMapChanged(ArcGISPortalItem portalItem)
        {
            if (portalItem == null)
            {
                return;
            }
            _navigationService.UriFor<WebMapViewModel>().WithParam(x => x.WebMapId, portalItem.Id).Navigate();
        }

        /// <summary>
        /// Load Recent items asynchronously
        /// </summary>
        public override async Task LoadContentAsync()
        {           
            try
            {
                var results = await _portalService.GetNewestOrganizationWebMapsAsync();
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
