namespace ArcOrganization.WebMap
{
    using System;
    using System.Collections.Generic;
    using System.Device.Location;
    using System.IO.IsolatedStorage;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Media.Imaging;

    using ArcOrganization.Infrastructure;
    using ArcOrganization.Infrastructure.Extensions;
    using ArcOrganization.Infrastructure.ViewModels;
    using ArcOrganization.Login;
    using ArcOrganization.Services;

    using Caliburn.Micro;

    using ESRI.ArcGIS.Client;
    using ESRI.ArcGIS.Client.Geometry;
    using ESRI.ArcGIS.Client.Portal;
    using ESRI.ArcGIS.Client.Tasks;
    using ESRI.ArcGIS.Client.Toolkit.DataSources;
    using ESRI.ArcGIS.Client.WebMap;

    using Microsoft.Phone.Shell;

    using GeometryService = ESRI.ArcGIS.Client.Tasks.GeometryService;
    using Query = ESRI.ArcGIS.Client.Tasks.Query;

    public class WebMapViewModel : WebMapScreenViewModel
    {
        private readonly PortalService _portalService;

        private readonly INavigationService _navigationService;

        private BindableCollection<ArcGISPortalItem> _basemaps;

        private ArcGISPortalItem _selectedBasemap;

        private BindableCollection<MapPortalItem> _searchResults;

        private MapPortalItem _selectedResult;

        public WebMapViewModel(PortalService portalService, INavigationService navigationService)
        {
            _portalService = portalService;
            _navigationService = navigationService;
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

        public ArcGISPortalItem CurrentItem { get; private set; }

        protected override void OnDeactivate(bool close)
        {
            Layers = new LayerCollection();
            base.OnDeactivate(close);
        }

        public void EnableGPS()
        {
            var gpsLayer = Layers.OfType<GpsLayer>().FirstOrDefault();
            if (gpsLayer == null)
            {
                gpsLayer = new GpsLayer();
                var geoCoordinateWatcher = new GeoCoordinateWatcher(GeoPositionAccuracy.Default)
                                               {
                                                   MovementThreshold = 10
                                               };
                gpsLayer.GeoPositionWatcher = geoCoordinateWatcher;
                gpsLayer.DisplayName = "Location";
                gpsLayer.IsEnabled = true;
                geoCoordinateWatcher.Start();
                Layers.Add(gpsLayer);
            }
            else
            {
                if (gpsLayer.IsEnabled)
                {
                    gpsLayer.IsEnabled = false;
                    gpsLayer.GeoPositionWatcher.Stop();
                }
                else
                {
                    gpsLayer.IsEnabled = true;
                    gpsLayer.GeoPositionWatcher.TryStart(true, new TimeSpan(0, 0, 30));
                }
            }
        }

        public override async Task LoadContentAsync()
        {
            try
            {
                await base.LoadContentAsync();
            }
            catch (ArgumentException exception)
            {
                if (exception.Message
                    == "You do not have permissions to access this resource or perform this operation.")
                {
                    _navigationService.UriFor<LoginViewModel>()
                                      .WithParam(p => p.WebMapId, WebMapId)
                                      .WithParam(p => p.RemoveFromBackstackNavigation, true)
                                      .Navigate();
                    return;
                }
                else
                {
                    throw;
                }
            }

            var foundBasemaps = await _portalService.LoadBasemapGalleryAsync();
            Basemaps = new BindableCollection<ArcGISPortalItem>(foundBasemaps);

            CurrentItem = await _portalService.GetPortalItemById(WebMapId);
        }

        public async Task<Graphic> QueryPoint(MapPoint point, string layerUrl, int layerId, Envelope extent)
        {
            try
            {
                var queryTask = new QueryTask(string.Format("{0}/{1}", layerUrl, layerId));
                var parameters = new Query { ReturnGeometry = true };
                parameters.OutFields.Add("*");

                var contractRatio = extent.Width / 20;
                var inputEnvelope = new Envelope(
                    point.X - contractRatio, point.Y - contractRatio, point.X + contractRatio, point.Y + contractRatio)
                                        {
                                            SpatialReference
                                                =
                                                point
                                                .SpatialReference
                                        };
                parameters.Geometry = inputEnvelope;

                var results = await queryTask.ExecuteTaskAsync(parameters);

                if (results.FeatureSet.Features.Count > 0)
                {
                    return results.FeatureSet.Features[0];
                }
                else
                {
                    return null;
                }
            }
            catch (Exception exception)
            {
                // TODO
                throw;
            }

            return null;
        }

        public async void ChangeBasemap(ArcGISPortalItem portalItem)
        {
            try
            {
                await BasemapChangedAsync(portalItem);
            }
            catch (Exception exception)
            {
                MessageService.ShowMessageAsync("Error occured on changing basemap.", "Ups!");
            }
        }

        public void CreateSecondaryTile()
        {
            // Create a filename for JPEG file in isolated storage.
            var tempJPEG = CurrentItem.Id + ".jpg";

            // Create virtual store and file stream. Check for duplicate tempJPEG files.
            using (var myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                var imageFolder = @"/Shared/ShellContent";
                if (!myIsolatedStorage.DirectoryExists(imageFolder))
                {
                    myIsolatedStorage.CreateDirectory(imageFolder);
                }

                if (myIsolatedStorage.FileExists(tempJPEG))
                {
                    myIsolatedStorage.DeleteFile(tempJPEG);
                }

                var filePath = System.IO.Path.Combine(imageFolder, tempJPEG);

                var fileStream = myIsolatedStorage.CreateFile(filePath);

                var bitmap = new BitmapImage
                                 {
                                     UriSource = CurrentItem.ThumbnailUri,
                                     CreateOptions = BitmapCreateOptions.None
                                 };
                var wb = new WriteableBitmap(bitmap);

                // Encode WriteableBitmap object to a JPEG stream.
                wb.SaveJpeg(fileStream, 366, 366, 0, 100);

                //wb.SaveJpeg(fileStream, wb.PixelWidth, wb.PixelHeight, 0, 85);
                fileStream.Close();
            }

            var schema = "isostore:/Shared/ShellContent/";
            var uriToStorage = new Uri(schema + tempJPEG, UriKind.Absolute);

            ShellTileData tile = new StandardTileData { Title = CurrentItem.Title, BackgroundImage = uriToStorage };
            var uriToView = string.Format("/WebMap/WebMapView.xaml?WebMapId={0}", WebMapId);

            ShellTile.Create(new Uri(uriToView, UriKind.Relative), tile);
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
                var layer = new FeatureLayer
                                {
                                    ID = serviceItem.Item.Id,
                                    DisplayName = serviceItem.Item.Title,
                                    Mode = FeatureLayer.QueryMode.OnDemand,
                                    Where = "1=1",
                                    Url = serviceItem.Item.Url + "/0",
                                    ShowLegend = true,
                                    ProjectionService =
                                        new GeometryService
                                            {
                                                Url =
                                                    UrlResources
                                                    .ArcGisOnlineGeometryServiceUrl
                                            }
                                };
                layer.InitializationFailed += (sender, args) =>
                    {
                        var l = sender as Layer;
                        MessageService.ShowMessageAsync(
                            string.Format("Layer : {0} couldn't be loaded.", l.DisplayName), "");
                    };
                layer.Initialized += (sender, args) =>
                    {
                        //(sender as FeatureLayer).Update();
                    };
                layer.UpdateCompleted += (sender, args) =>
                    {
                        // 
                    };
                layer.UpdateFailed += (sender, args) =>
                    {
                        var l = sender as Layer;
                        MessageService.ShowMessageAsync(string.Format("Layer : {0} update failed.", l.DisplayName), "");
                    };

                layer.SetPortalId(serviceItem.Item.Id);
                Layers.Add(layer);
                NotifyOfPropertyChange(() => Layers);
            }
        }
    }
}