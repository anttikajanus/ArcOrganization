namespace ArcOrganization.Infrastructure
{
    using System.Windows;
    using System.Collections.Generic;

    using ESRI.ArcGIS.Client;
    using ESRI.ArcGIS.Client.WebMap;

    public static class LayerUtilities
    {
        /// <summary>
        /// Get <see cref="DataTemplate"/> for the <see cref="ArcGISDynamicMapServiceLayer"/>.
        /// </summary>
        /// <param name="layer">Dynamic layer.</param>
        /// <param name="mapScale">Map scale.</param>
        /// <returns></returns>
        public static DataTemplate GetPopupDataTemplate(this ArcGISDynamicMapServiceLayer layer, double mapScale)
        {
            var layId = 0;

            var dynamicLayer = layer;
            if (dynamicLayer != null)
            {
                var idict = layer.GetValue(Document.PopupTemplatesProperty) as IDictionary<int, DataTemplate>;
                for (var i = layId; i < dynamicLayer.Layers.Length; i++)
                {
                    var linfo = dynamicLayer.Layers[i];
                    if (((mapScale > linfo.MaxScale // in scale range
                          && mapScale < linfo.MinScale) || (linfo.MaxScale == 0.0 // no scale dependency
                                                            && linfo.MinScale == 0.0)
                         || (mapScale > linfo.MaxScale // minscale = 0.0 = infinity
                             && linfo.MinScale == 0.0)) && idict.ContainsKey(linfo.ID)) // id present in dictionary
                    {
                        // Get template from target layer with id.
                        var dataTemplate = idict[linfo.ID];
                        return dataTemplate;
                    }
                }
            }

            return null;
        }
       
    }
}
