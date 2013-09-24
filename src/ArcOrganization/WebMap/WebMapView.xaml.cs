namespace ArcOrganization.WebMap
{
    using System;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Linq;

    using ArcOrganization.Infrastructure;
    using ArcOrganization.Infrastructure.Extensions;

    using ESRI.ArcGIS.Client;
    using ESRI.ArcGIS.Client.Geometry;
    using ESRI.ArcGIS.Client.Toolkit.DataSources;
    using ESRI.ArcGIS.Client.WebMap;

    public partial class WebMapView
    {
        public WebMapView()
        {
            InitializeComponent();
        }

        private void OnGesture(object sender, Map.MapGestureEventArgs e)
        {
            if (e.Gesture == GestureType.Tap)
            {
                QueryToServices(e, MyMap.Layers.Count-1);
            }
        }

        private void QueryToServices(Map.MapGestureEventArgs e, int layerId)
        {
            MyInfoWindow.IsOpen = false;
            var mapScale = (MyMap.Resolution * 39.3700787) * 96;

            for (int i = layerId; i > 0; i--)
            {
                var layer = MyMap.Layers[i];

                // If layer is not visible, skip it.
                if (layer.Visible == false || layer.Opacity.Equals(0))
                {
                    continue;
                }

                if (layer is FeatureLayer)
                {
                    // Layer is graphics layer   
                    var graphicLayer = layer as FeatureLayer;
                    var clickPoint = e.MapPoint;

                    // Get layer that we are working with and get features that are inside of the click + 10 pixel radious
                    var g = e.DirectlyOver(10, new GraphicsLayer[] { graphicLayer });

                    if (!g.Any())
                    {
                        // No graphics found from that location so continue to next layer.
                        continue;
                    }

                    DataTemplate dataTemplate;
                    if (graphicLayer.GetValue(Document.PopupTemplateProperty) != null)
                    {
                        // Layer has template, get it and show graphic in it.
                        dataTemplate = graphicLayer.GetValue(Document.PopupTemplateProperty) as DataTemplate;
                        MyInfoWindow.ContentTemplate = dataTemplate;

                        var graphic = g.ToList()[0];
                        MyInfoWindow.Content = graphic.Attributes;
                        if (graphic.Geometry is MapPoint)
                        {
                            MyInfoWindow.Anchor = graphic.Geometry as MapPoint;
                        }
                        else
                        {
                            MyInfoWindow.Anchor = clickPoint;
                        }
                        MyInfoWindow.IsOpen = true;

                    }
                    else
                    {
                        // Check if layer is added to the map in app, if yes,  then use default template
                        // If layer is part of the webmap and there is no popup definition, don't show any results.
                        var layersPortalId = layer.GetPortalId();
                        if (!string.IsNullOrEmpty(layersPortalId))
                        {
                            // Layer doesn't have template
                            dataTemplate = Resources["PopupTemplate"] as DataTemplate;
                            MyInfoWindow.ContentTemplate = dataTemplate;

                            var graphic = g.ToList()[0];
                            MyInfoWindow.Content = graphic;
                            if (graphic.Geometry is MapPoint)
                            {
                                MyInfoWindow.Anchor = graphic.Geometry as MapPoint;
                            }
                            else
                            {
                                MyInfoWindow.Anchor = clickPoint;
                            }
                            MyInfoWindow.IsOpen = true;
                        }
                    }
                }
                else if (layer is ArcGISDynamicMapServiceLayer)
                {
                    // Layer is dynamic layer
                    var dynamicLayer = layer as ArcGISDynamicMapServiceLayer;

                    var dynamicDataTemplate = dynamicLayer.GetPopupDataTemplate(mapScale);
                    if (dynamicDataTemplate == null)
                    {
                        // Layer doesn't have template so continue to next layer.
                        continue;
                    }

                    MyInfoWindow.ContentTemplate = dynamicDataTemplate;

                    // Execute query, if attributes were not found, call QueryToServices for next layer.
                    ExecuteQuery(e, dynamicDataTemplate, dynamicLayer, i);
                    return;
                }
            }
        }

        private async Task ExecuteQuery(
            Map.MapGestureEventArgs e, DataTemplate dt, ArcGISDynamicMapServiceLayer alayer, int layid)
        {
            if (dt != null)
            {
                var result = await ((WebMapViewModel)DataContext).QueryPoint(e.MapPoint, alayer.Url, layid, MyMap.Extent);
                if (result != null)
                {
                    MyInfoWindow.Anchor = result.Geometry as MapPoint;
                    DetailsContainer.ContentTemplate = dt;
                    DetailsContainer.Content = result;
                    MyInfoWindow.IsOpen = true;
                }
                else
                {
                    QueryToServices(e, layid + 1);
                }
            }
        }

        private void OpenTableOfContents(object sender, EventArgs e)
        {
            TableOfContentPage.IsOpen = !TableOfContentPage.IsOpen;
        }

        //private void PanToGPS(object sender, EventArgs e)
        //{
        //    var gpsLayer = MyMap.Layers.OfType<GpsLayer>().FirstOrDefault();
        //    if (gpsLayer == null)
        //    {
        //        return;
        //    }

        //    MyMap.PanTo(gpsLayer.Position);
        //}

        private void OpenDetails(object sender, RoutedEventArgs e)
        {
            DetailsPage.IsOpen = true;
        }

        private void ChangeWebMap(object sender, EventArgs e)
        {
            BaseMapsPage.IsOpen = !BaseMapsPage.IsOpen;
        }

        private void OpenSearchPage(object sender, EventArgs e)
        {
            SearchServicesPage.IsOpen = !SearchServicesPage.IsOpen;
        }
    }
}