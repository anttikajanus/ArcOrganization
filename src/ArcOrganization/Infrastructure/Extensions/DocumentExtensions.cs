namespace ArcOrganization.Infrastructure.Extensions
{
    using System.Threading.Tasks;

    using ESRI.ArcGIS.Client.WebMap;

    public static class DocumentExtensions
    {
        /// <summary>
        /// Loads WebMap by given item Id asynchronously.
        /// </summary>
        /// <param name="document">The Document used.</param>
        /// <param name="webmapId">The WebMap id that is loaded.</param>
        /// <returns>Returns <see cref="GetMapCompletedEventArgs"/> as a <see cref="Task{T}"/></returns>
        public static Task<GetMapCompletedEventArgs> LoadWebMapAsync(this Document document, string webmapId)
        {
            var tcs = new TaskCompletionSource<GetMapCompletedEventArgs>();

            document.GetMapCompleted += (sender, e) =>
                {
                    if (e.Error != null)
                    {
                        tcs.SetException(e.Error);
                        return;
                    }

                    tcs.SetResult(e);
                };

            document.GetMapAsync(webmapId);
            return tcs.Task;
        }
    }
}