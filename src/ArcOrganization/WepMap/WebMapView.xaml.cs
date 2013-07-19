using System;
using System.Windows;

using Microsoft.Phone.Controls;

namespace ArcOrganization.WepMap
{
    using System.Collections.Generic;
    using System.Linq;

    using ESRI.ArcGIS.Client;
    using ESRI.ArcGIS.Client.Geometry;
    using ESRI.ArcGIS.Client.Tasks;
    using ESRI.ArcGIS.Client.Toolkit.DataSources;
    using ESRI.ArcGIS.Client.WebMap;

    public partial class WebMapView : PhoneApplicationPage
    {
        public WebMapView()
        {
            InitializeComponent();
        }

        private void OnGesture(object sender, Map.MapGestureEventArgs e)
        {
            if (e.Gesture == GestureType.Tap)
            {
                QueryToServices(e);
            }
        }

        private void QueryToServices(Map.MapGestureEventArgs e, int layerId = 0)
        {
            MyInfoWindow.IsOpen = false;

            var mapScale = (MyMap.Resolution * 39.3700787) * 96;

            ArcGISDynamicMapServiceLayer alayer = null;
            DataTemplate dt = null;
            var layid = layerId;

            foreach (var layer in MyMap.Layers)
            {
                if (layer.GetValue(Document.PopupTemplatesProperty) == null)
                {
                    continue;
                }

                alayer = layer as ArcGISDynamicMapServiceLayer;

                var idict = layer.GetValue(Document.PopupTemplatesProperty) as IDictionary<int, DataTemplate>;
                for (var i = layid; i < alayer.Layers.Length; i++)
                {
                    var linfo = alayer.Layers[i];
                    if (((mapScale > linfo.MaxScale // in scale range
                          && mapScale < linfo.MinScale) || (linfo.MaxScale == 0.0 // no scale dependency
                                                            && linfo.MinScale == 0.0)
                         || (mapScale > linfo.MaxScale // minscale = 0.0 = infinity
                             && linfo.MinScale == 0.0)) && idict.ContainsKey(linfo.ID)) // id present in dictionary
                    {
                        layid = linfo.ID;
                        dt = idict[linfo.ID];
                        break;
                    }
                }
                break;
            }
            if (dt != null)
            {
                ExecuteQuery(e, dt, alayer, layid);
                return;
            }

            foreach (var layer in MyMap.Layers)
            {
                if (layer.GetValue(Document.PopupTemplateProperty) == null)
                {
                    continue;
                }

                var flayer = layer as FeatureLayer;
                var clickPoint = e.MapPoint;

                if (flayer.GetValue(Document.PopupTemplateProperty) != null)
                {
                    // Get layer that we are working with and get features that are inside of the click + 4 pixel radious
                    var g = e.DirectlyOver(10, new GraphicsLayer[] { flayer });

                    if (!g.Any())
                    {
                        continue;
                    }

                    var fdt = flayer.GetValue(Document.PopupTemplateProperty) as DataTemplate;
                    MyInfoWindow.ContentTemplate = fdt;

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
            }
        }

        private void ExecuteQuery(
            Map.MapGestureEventArgs e, DataTemplate dt, ArcGISDynamicMapServiceLayer alayer, int layid)
        {
            if (dt != null)
            {
                var qt = new QueryTask(string.Format("{0}/{1}", alayer.Url, layid));
                qt.ExecuteCompleted += (s, qe) =>
                    {
                        if (qe.FeatureSet.Features.Count > 0)
                        {
                            var g = qe.FeatureSet.Features[0];
                            MyInfoWindow.Anchor = g.Geometry as MapPoint;

                            DetailsContainer.ContentTemplate = dt;
                            DetailsContainer.Content = g.Attributes;

                            InfoTitle.Text = g.Attributes["FID"].ToString();

                            MyInfoWindow.IsOpen = true;
                        }
                        else
                        {
                            QueryToServices(e, layid + 1);
                        }
                    };

                var query = new ESRI.ArcGIS.Client.Tasks.Query();
                var contractRatio = MyMap.Extent.Width / 20;
                var inputEnvelope = new Envelope(
                    e.MapPoint.X - contractRatio,
                    e.MapPoint.Y - contractRatio,
                    e.MapPoint.X + contractRatio,
                    e.MapPoint.Y + contractRatio) { SpatialReference = MyMap.SpatialReference };
                query.ReturnGeometry = true;
                query.Geometry = inputEnvelope;
                query.OutSpatialReference = MyMap.SpatialReference;
                query.OutFields.Add("*");

                qt.ExecuteAsync(query);
            }
        }

        private void OpenTableOfContents(object sender, EventArgs e)
        {
            TableOfContentPage.IsOpen = !TableOfContentPage.IsOpen;
        }

        private void PanToGPS(object sender, EventArgs e)
        {
            var gpsLayer = MyMap.Layers.OfType<GpsLayer>().FirstOrDefault();
            if (gpsLayer == null)
            {
                return;
            }

            MyMap.PanTo(gpsLayer.Position);
        }

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