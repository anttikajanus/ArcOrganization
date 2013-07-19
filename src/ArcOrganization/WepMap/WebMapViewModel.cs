namespace ArcOrganization.WepMap
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;

    using ArcOrganization.Infrastructure;
    using ArcOrganization.Infrastructure.Extensions;
    using ArcOrganization.Infrastructure.ViewModels;
    using ArcOrganization.Services;

    using Caliburn.Micro;

    using ESRI.ArcGIS.Client;
    using ESRI.ArcGIS.Client.Portal;
    using ESRI.ArcGIS.Client.Tasks;
    using ESRI.ArcGIS.Client.WebMap;

    using GeometryService = ESRI.ArcGIS.Client.Tasks.GeometryService;

    public class WebMapViewModel : WebMapScreenViewModel
    {
        private readonly PortalService _portalService;

        private BindableCollection<ArcGISPortalItem> _basemaps;

        private ArcGISPortalItem _selectedBasemap;

        private BindableCollection<MapPortalItem> _searchResults;

        private MapPortalItem _selectedResult;

        public WebMapViewModel(PortalService portalService)
        {
            _portalService = portalService;
        }

        public ArcGISPortalItem SelectedBasemap
        {
            get
            {
                return _selectedBasemap;
            }

            set
            {
                if (Equals(value, _selectedBasemap))
                {
                    return;
                }
                _selectedBasemap = value;
                ChangeBasemap(value);
                NotifyOfPropertyChange(() => SelectedBasemap);
            }
        }

        /// <summary>
        /// Gets or sets the new maps as a ArcGIS Portal Items 
        /// </summary>
        public BindableCollection<ArcGISPortalItem> Basemaps
        {
            get
            {
                return _basemaps;
            }

            set
            {
                if (Equals(value, _basemaps))
                {
                    return;
                }
                _basemaps = value;
                NotifyOfPropertyChange(() => Basemaps);
            }
        }

        public BindableCollection<MapPortalItem> SearchResults
        {
            get
            {
                return _searchResults;
            }
            set
            {
                if (Equals(value, _searchResults))
                {
                    return;
                }
                _searchResults = value;
                NotifyOfPropertyChange(() => SearchResults);
            }
        }

        public MapPortalItem SelectedResult
        {
            get
            {
                return _selectedResult;
            }
            set
            {
                if (Equals(value, _selectedResult))
                {
                    return;
                }
                _selectedResult = value;

                if (_searchResults != null)
                {
                    if (_selectedResult.IsOnMap)
                    {
                        RemoveServiceFromMap(_selectedResult);
                    }
                    else
                    {
                        AddServiceToMap(_selectedResult);
                    }
                    _selectedResult.IsOnMap = !_selectedResult.IsOnMap;
                   
                }

                NotifyOfPropertyChange(() => SelectedResult);
            }
        }

        public override async Task LoadContentAsync()
        {
            await base.LoadContentAsync();
            var foundBasemaps = await _portalService.LoadBasemapGalleryAsync();
            Basemaps = new BindableCollection<ArcGISPortalItem>(foundBasemaps);

            // Get default search
            var defaultSearch = await _portalService.GetNewestOrganizationFeatureServicesAsync();
            SearchResults = new BindableCollection<MapPortalItem>();
            defaultSearch.ToList().ForEach(x => SearchResults.Add(new MapPortalItem { Item = x, IsOnMap = false }));
        }

        public async void ChangeBasemap(ArcGISPortalItem portalItem)
        {
            try
            {
                await BasemapChangedAsync(portalItem);
            }
            catch (Exception exception)
            {
                // TODO
                throw;
            }
        }

        /// <summary>
        /// Loads selected basemap and changes basemap layers from the used LayersCollection
        /// </summary>
        /// <param name="basemapItem">Selected basemap item.</param>
        private async Task BasemapChangedAsync(ArcGISPortalItem basemapItem)
        {
            var map = await _portalService.LoadWebmapByIdAsync(basemapItem.Id);
            var newBasemapLayers = new LayerCollection();

            // Clear basemaps from the layers
            foreach (var layer in Layers.ToList().Where(Document.GetIsBaseMap))
            {
                Layers.Remove(layer);
            }

            // Get new basemap layers
            foreach (var layer in map.Layers.Where(Document.GetIsBaseMap))
            {
                newBasemapLayers.Add(layer);
            }

            // Clearing reference to existing map
            map.Layers.Clear();

            var index = 0;
            foreach (var layer in newBasemapLayers)
            {
                // Set to true to enable legend usage.
                layer.ShowLegend = true;

                // Check if layer is a reference layer (ie labels that go on top)
                var data = layer.GetValue(Document.WebMapDataProperty) as IDictionary<string, object>;
                if (!data.ContainsKey("isReference"))
                {
                    Layers.Insert(index++, layer);
                }
                else
                {
                    // Reference layers go on top.
                    Layers.Add(layer);
                }
            }
        }

        private void RemoveServiceFromMap(MapPortalItem serviceItem)
        {
            var existingLayers = Layers.Where(x => x.DisplayName == serviceItem.Item.Title).ToList();

            // Remove existing layers that are loaded from service, there should be one but if some reason item gets added more than once...
            if (existingLayers.Count > 0)
            {
                existingLayers.ForEach(x => Layers.Remove(x));
            }
        }

        private async void AddServiceToMap(MapPortalItem serviceItem)
        {
            var existingLayers = Layers.Where(x => x.DisplayName == serviceItem.Item.Title).ToList();
            if (existingLayers.Count > 0)
            {
                // item is already there
                return;
            }

            if (serviceItem.Item.Type == ItemType.FeatureService)
            {
                var layer = new FeatureLayer();
                layer.ID = serviceItem.Item.Id;
                layer.DisplayName = serviceItem.Item.Title;
                layer.Mode = FeatureLayer.QueryMode.OnDemand;
                layer.Where = "1=1";
                layer.Url = serviceItem.Item.Url+ "/0";
                layer.ShowLegend = true;
                layer.ProjectionService = new GeometryService() { Url = UrlResources.ArcGisOnlineGeometryServiceUrl };
                layer.InitializationFailed += (sender, args) => { string x = "yo"; };
                layer.Initialized += (sender, args) => { (sender as FeatureLayer).Update(); };
                layer.UpdateCompleted +=
                    (sender, args) =>
                        {
                            //MessageBox.Show((sender as FeatureLayer).Graphics.Count.ToString());
                        };
                layer.UpdateFailed += (sender, args) =>
                    {
                        //MessageBox.Show(args.Error.ToString());
                    };

                Layers.Add(layer);
                NotifyOfPropertyChange(() => Layers);
            }
        }
    }
}