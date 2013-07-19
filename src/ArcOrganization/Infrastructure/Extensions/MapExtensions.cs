namespace ArcOrganization.Infrastructure.Map
{
    using System;
    using System.Windows;

    using ArcOrganization.Infrastructure.Extensions;

    using ESRI.ArcGIS.Client;
    using ESRI.ArcGIS.Client.Geometry;

    public class MapExtensions : DependencyObject
    {
        public static readonly DependencyProperty ExtentProperty = DependencyProperty.RegisterAttached(
            "Extent", typeof(Envelope), typeof(MapExtensions), new PropertyMetadata(PropertyChangedCallback));

        private async static void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var map = dependencyObject as Map;
            if (map == null)
            {
                return;
            }

            if (map.SpatialReference == null)
            {
                return;
            }

            var newExtent = dependencyPropertyChangedEventArgs.NewValue as Envelope;

            // If spatial reference differs, reproject it.
            if (newExtent.SpatialReference != map.SpatialReference)
            {
               try
               {
                   var projectedExtent = await GeometryServiceExtensions.ProjectAsync(newExtent, map.SpatialReference);
                   map.ZoomTo(projectedExtent as Envelope);
               }
               catch (Exception exception)
               {
                   //TODO
               }
            }
            else
            {
                map.ZoomTo(newExtent);
            }
        }

        public static void SetExtent(UIElement element, Envelope value)
        {
            element.SetValue(ExtentProperty, value);
        }

        public static Envelope GetExtent(UIElement element)
        {
            return (Envelope)element.GetValue(ExtentProperty);
        }
    }
}
