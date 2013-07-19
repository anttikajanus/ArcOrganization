namespace ArcOrganization.Infrastructure.ViewModels
{
    using System.Device.Location;
    using System.Linq;
    using System.Threading.Tasks;

    using ArcOrganization.Infrastructure.Extensions;

    using ESRI.ArcGIS.Client;
    using ESRI.ArcGIS.Client.Toolkit.DataSources;
    using ESRI.ArcGIS.Client.WebMap;

    public class WebMapScreenViewModel : MapScreenViewModel
    {
        private string _webMapId;

        private bool _isLayersLoaded;

        public string WebMapId
        {
            get
            {
                return _webMapId;
            }

            set
            {
                if (value == _webMapId)
                {
                    return;
                }
                _webMapId = value;
                NotifyOfPropertyChange(() => WebMapId);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the layers are loaded.
        /// </summary>
        public bool IsLayersLoaded
        {
            get
            {
                return _isLayersLoaded;
            }

            set
            {
                if (value.Equals(_isLayersLoaded))
                {
                    return;
                }
                _isLayersLoaded = value;
                NotifyOfPropertyChange(() => IsLayersLoaded);
            }
        }

        public override async Task LoadContentAsync()
        {
            var basemapLayer = new ArcGISTiledMapServiceLayer()
                                   {
                                       Url =
                                           @"http://services.arcgisonline.com/ArcGIS/rest/services/World_Topo_Map/MapServer",
                                       ID = "Basemap",
                                       DisplayName = "Topographic map"
                                   };
            basemapLayer.SetValue(Document.IsBaseMapProperty, true);
            Layers.Add(basemapLayer);

            await LoadWebMap(WebMapId);

            var gpsLayer = new GpsLayer();
            var geoCoordinateWatcher = new GeoCoordinateWatcher(GeoPositionAccuracy.Default) { MovementThreshold = 10 };
            gpsLayer.GeoPositionWatcher = geoCoordinateWatcher;
            gpsLayer.DisplayName = "Location";
            gpsLayer.IsEnabled = true;
            geoCoordinateWatcher.Start();
            Layers.Add(gpsLayer);
        }

        /// <summary>
        /// Load new WebMap to the application.
        /// </summary>
        /// <param name="id">The Id of the webmap.</param>
        private async Task LoadWebMap(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return;
            }

            var webMapDocument = new Document();

            // Get the Map from the ArcGIS Online
            var result = await webMapDocument.LoadWebMapAsync(id);
            Extent = result.ItemInfo.Extent;

            // Find all layers that are added from WebMaps.
            var existingLayers = Layers.Where(x => !string.IsNullOrEmpty(x.GetWebMapId())).ToList();

            // Remove existing layers that are loaded from other WebMap
            if (existingLayers.Count > 0)
            {
                existingLayers.ForEach(x => Layers.Remove(x));
            }

            // Iterate through all layers that are not basemaps and add them to
            var layersToAdd = result.Map.Layers.Where(x => !Document.GetIsBaseMap(x)).ToList();

            // Remove layer references from loaded map
            result.Map.Layers.Clear();

            // Add loaded layers to the layers used
            foreach (var layer in layersToAdd)
            {
                Layers.Add(layer);
            }

            
        }
    }
}