namespace ArcOrganization.Infrastructure.Extensions
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using ESRI.ArcGIS.Client;
    using ESRI.ArcGIS.Client.Geometry;
    using ESRI.ArcGIS.Client.Tasks;

    public static class GeometryServiceExtensions
    {
        /// <summary>
        /// Project geometry to target <see cref="SpatialReference"/>.
        /// </summary>
        /// <param name="geometry">The geometry to project.</param>
        /// <param name="spatialReference">Target spatial reference.</param>
        /// <returns>Returns projected geometry.</returns>
        public static Task<Geometry> ProjectAsync(Geometry geometry, SpatialReference spatialReference)
        {
            var tcs = new TaskCompletionSource<Geometry>();

            var geometryService = new GeometryService(UrlResources.ArcGisOnlineGeometryServiceUrl);

            geometryService.ProjectCompleted += (sender, args) => tcs.SetResult(args.Results[0].Geometry);
            geometryService.Failed += (sender, args) => tcs.SetException(args.Error);

            geometryService.ProjectAsync(new List<Graphic> { new Graphic { Geometry = geometry } }, spatialReference);

            return tcs.Task;
        }
    }
}
