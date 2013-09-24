namespace ArcOrganization.Services
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    using ArcOrganization.Infrastructure.Extensions;
    using ArcOrganization.Settings;

    using Caliburn.Micro;

    using ESRI.ArcGIS.Client;
    using ESRI.ArcGIS.Client.Portal;
    using ESRI.ArcGIS.Client.WebMap;

    public class PortalService : PropertyChangedBase 
    {
        private readonly ISettingsStore _settingsStore;

        private ArcGISPortal _portal;

        private ArcGISPortalUser _userInformation;

        private List<ArcGISPortalItem> _organizationalWebMapsFromCache; 
        private List<ArcGISPortalItem> _loadedWebMapItems; 

        public PortalService(ISettingsStore settingsStore)
        {
            _settingsStore = settingsStore;
            _portal = new ArcGISPortal();
            _loadedWebMapItems = new List<ArcGISPortalItem>();
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
            //var basemapsFromCache = _settingsStore.BasemapItems;
            //if (basemapsFromCache.Count > 0)
            //{
            //    return basemapsFromCache;
            //}

            var parameters = new SearchParameters() { Limit = limit };

            // Load ArcGISPortalItems from the Online that contains basemaps.
            var items = await _portal.LoadBasemapGalleryAsync(parameters);

            /// Note that basemaps provided by the Esri Finland doesn't contain Name value
            var filteredItems = items.Where(basemap => !string.IsNullOrEmpty(basemap.Name)).ToList();
            var results  = filteredItems.Where(l => !l.Name.Contains("Bing"));

            //_settingsStore.BasemapItems = results.ToList();
            //_settingsStore.BasemapItemsSaved = DateTime.Now;

            // Filter away Bing maps since we don't have a token.
            return results;
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

        public async Task<IEnumerable<ArcGISPortalItem>> GetNewestOrganizationWebMapsAsync(int limit = 10, bool cache = true)
        {
            if (!_portal.IsInitialized)
            {
                throw new Exception("Portal is not initialized.");
            }

            IEnumerable<ArcGISPortalItem> results;
            //IEnumerable<ArcGISPortalItem> results = await Task.Run(
            //    async () =>
            //        {

            Debug.WriteLine(DateTime.Now.ToLongTimeString());

            //if (_settingsStore.NewestOrganizationWebMapsSaved > DateTime.Now.AddHours(-1))
            //{
            //    if (_organizationalWebMapsFromCache == null)
            //    {
            //        // Get items only once from cache.
            //        _organizationalWebMapsFromCache = _settingsStore.NewestOrganizationWebMapItems;
            //    }
            //    if (_organizationalWebMapsFromCache.Count > 0)
            //    {
            //        foreach (var item in _organizationalWebMapsFromCache)
            //        {

            //        }
            //        Debug.WriteLine(DateTime.Now.ToLongTimeString());
            //        return _organizationalWebMapsFromCache;
            //    }
            //}

                        var parameters = new SearchParameters
                                             {
                                                 Limit = limit,
                                                 QueryString =
                                                     string.Format(
                                                         "accountid:\"{0}\" AND type:\"Web Map\" AND -type:\"Web Mapping Application\"",
                                                         _portal.CurrentUser.OrgId),
                                                 SortField = "uploaded",
                                                 SortOrder = QuerySortOrder.Descending
                                             };

                        var searchResults = await _portal.SearchItemsAsync(parameters);
                        _loadedWebMapItems.AddRange(searchResults);
                        //if (cache)
                        //{
                        //    Task.Factory.StartNew(
                        //        () =>
                        //            {
                        //                _settingsStore.NewestOrganizationWebMapItems = searchResults.ToList();
                        //                _settingsStore.NewestOrganizationWebMapsSaved = DateTime.Now;
                        //            });
                            
                        //}
                        Debug.WriteLine(DateTime.Now.ToLongTimeString());

                        return searchResults;
                    //});

            return results;
        }

        public async Task<IEnumerable<ArcGISPortalItem>> GetNewestOrganizationFeatureServicesAsync(int limit = 10)
        {
            if (!_portal.IsInitialized)
            {
                throw new Exception("Portal is not initialized.");
            }
            
            IEnumerable<ArcGISPortalItem> results = await Task.Run(async () =>
            {
                var parameters = new SearchParameters
                {
                    Limit = limit,
                    QueryString =
                        string.Format(
                            "accountid:\"{0}\" AND type:\"Feature Service\"",
                            _portal.CurrentUser.OrgId),
                    SortField = "uploaded",
                    SortOrder = QuerySortOrder.Descending
                };

                return await _portal.SearchItemsAsync(parameters);
            });

            return results;
        }

        public async Task<ArcGISPortalItem> GetPortalItemById(string id)
        {
            if (!_portal.IsInitialized)
            {
                throw new Exception("Portal is not initialized.");
            }

            var _cachedItem = _loadedWebMapItems.FirstOrDefault(item => item.Id == id);
            if (_cachedItem != null)
            {
                return _cachedItem;
            }

            var parameters = new SearchParameters
            {
                Limit = 1,
                QueryString = string.Format("id:\"{0}\"", id)
            };

            var results = await _portal.SearchItemsAsync(parameters);
            if (results.Count() == 1)
            {
                var result = results.ToList()[0];
                _loadedWebMapItems.Add(result);
                return result;
            }
            
            return null;
        }
    }
}
