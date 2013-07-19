namespace ArcOrganization.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using ArcOrganization.Infrastructure.Extensions;

    using Caliburn.Micro;

    using ESRI.ArcGIS.Client;
    using ESRI.ArcGIS.Client.Portal;
    using ESRI.ArcGIS.Client.WebMap;

    public class PortalService : PropertyChangedBase 
    {
        private ArcGISPortal _portal;

        private ArcGISPortalUser _userInformation;

        public PortalService()
        {
            _portal = new ArcGISPortal();
        }

        public async Task<ArcGISPortalUser> GetUserInformationAsync()
        {
            if (!_portal.IsInitialized)
            {
                throw new Exception("Portal is not initialized.");
            }

            return _portal.CurrentUser;
        }
        
        public async Task InitializeAsync(string username, string password)
        {
            if (_portal.IsInitialized)
            {
                return;
            }

            try
            {
                var credential = await IdentityManager.Current.GetCredentialsAsync(username, password, "ArcOrganization");
                _portal.Credentials = credential.Credentials;

                 await _portal.InitializeAsync();
            }
            catch (Exception exception)
            {
                throw;
            }
        }

        public async Task<IEnumerable<ArcGISPortalItem>> LoadBasemapGalleryAsync(int limit = 10)
        {
            var parameters = new SearchParameters() { Limit = limit };

            // Load ArcGISPortalItems from the Online that contains basemaps.
            var items = await _portal.LoadBasemapGalleryAsync(parameters);

            /// Note that basemaps provided by the Esri Finland doesn't contain Name value
            var filteredItems = items.Where(basemap => !string.IsNullOrEmpty(basemap.Name)).ToList();

            // Filter away Bing maps since we don't have a token.
            return filteredItems.Where(l => !l.Name.Contains("Bing"));
        }

        public async Task<Map> LoadWebmapByIdAsync(string id)
        {
            var webMapDocument = new Document { Token = _portal.Token };

            // Load Map from the ArcGIS Online.
            var result = await webMapDocument.LoadWebMapAsync(id);

            // Set WebMap origin information to the layers.
            result.Map.Layers.ToList().ForEach(x => x.SetWebMapId(id));
            return result.Map;
        }

        public async Task<IEnumerable<ArcGISPortalItem>> GetNewestOrganizationWebMapsAsync(int limit = 10)
        {
            if (!_portal.IsInitialized)
            {
                throw new Exception("Portal is not initialized.");
            }

            var parameters = new SearchParameters
            {
                Limit = limit,
                QueryString = string.Format("accountid:\"{0}\" AND type:\"Web Map\" AND -type:\"Web Mapping Application\"", _portal.CurrentUser.OrgId),
                SortField = "uploaded",
                SortOrder = QuerySortOrder.Descending
            };

            return await _portal.SearchItemsAsync(parameters);
        }

        public async Task<IEnumerable<ArcGISPortalItem>> GetNewestOrganizationFeatureServicesAsync(int limit = 10)
        {
            if (!_portal.IsInitialized)
            {
                throw new Exception("Portal is not initialized.");
            }

            var parameters = new SearchParameters
            {
                Limit = limit,
                QueryString = string.Format("accountid:\"{0}\" AND type:\"Feature Service\"", _portal.CurrentUser.OrgId),
                SortField = "uploaded",
                SortOrder = QuerySortOrder.Descending
            };

            return await _portal.SearchItemsAsync(parameters);
        }
    }
}
