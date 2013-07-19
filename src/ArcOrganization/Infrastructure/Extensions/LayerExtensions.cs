namespace ArcOrganization.Infrastructure.Extensions
{
    using System.Windows;

    using ESRI.ArcGIS.Client;

    public static class LayerExtensions
    {
        /// <summary>
        /// Gets the WebMap id attached to the layer.
        /// </summary>
        /// <param name="layer">The layer where the id is looked from.</param>
        /// <returns>Returns WebMaps Id as a string. Empty if is not found.</returns>
        public static string GetWebMapId(this Layer layer)
        {
            return GetLayersWebMapId(layer);
        }

        /// <summary>
        /// Sets the WebMap id to the layer.
        /// </summary>
        /// <param name="layer">The layer where the id is attached.</param>
        /// <param name="id">The id.</param>
        public static void SetWebMapId(this Layer layer, string id)
        {
            SetLayersWebMapId(layer, id);
        }

        public static readonly DependencyProperty LayersWebMapIdProperty = DependencyProperty.RegisterAttached(
            "LayersWebMapId", typeof(string), typeof(LayerExtensions), new PropertyMetadata(default(string)));

        public static void SetLayersWebMapId(Layer layer, string value)
        {
            layer.SetValue(LayersWebMapIdProperty, value);
        }

        public static string GetLayersWebMapId(Layer layer)
        {
            return (string)layer.GetValue(LayersWebMapIdProperty);
        }


    }
}
