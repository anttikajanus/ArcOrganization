using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArcOrganization.Infrastructure.Extensions
{
    using ESRI.ArcGIS.Client.Portal;

    public static class PortalExtensions
    {
        /// <summary>
        /// Initializes the <see cref="ArcGISPortal"/>.
        /// </summary>
        /// <param name="portal">The portal instance that is initialized</param>
        /// <returns>Returns <see cref="Task{ArcGISPortal}"/>.</returns>
        public static Task<ArcGISPortal> InitializeAsync(this ArcGISPortal portal)
        {
            var tcs = new TaskCompletionSource<ArcGISPortal>();

            portal.InitializeAsync(
                UrlResources.ArcGISPortalUrl,
                (arcGisPortal, e) =>
                    {
                        if (e != null)
                        {
                            tcs.SetException(e);
                            return;
                        }

                        tcs.SetResult(arcGisPortal);
                    });

            return tcs.Task;
        }

        /// <summary>
        /// Searches <see cref="ArcGISPortalItem"/>s.
        /// </summary>
        /// <param name="portal">The portal where the search is executed.</param>
        /// <param name="parameters">The <see cref="SearchParameters"/> that defines the query.</param>
        /// <returns>Returns <see cref="Task"/> that contains the search results.</returns>
        public static Task<IEnumerable<ArcGISPortalItem>> SearchItemsAsync(this ArcGISPortal portal, SearchParameters parameters)
        {
            var tcs = new TaskCompletionSource<IEnumerable<ArcGISPortalItem>>();

            portal.SearchItemsAsync(
                parameters,
                (info, e) =>
                {
                    if (e != null)
                    {
                        tcs.SetException(e);
                        return;
                    }

                    tcs.SetResult(info.Results);
                });

            return tcs.Task;
        }

        /// <summary>
        /// Loads basemap gallery items.
        /// </summary>
        /// <param name="portal"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static Task<IEnumerable<ArcGISPortalItem>> LoadBasemapGalleryAsync(this ArcGISPortal portal, SearchParameters parameters)
        {
            var tcs = new TaskCompletionSource<IEnumerable<ArcGISPortalItem>>();

            portal.ArcGISPortalInfo.SearchBasemapGalleryAsync(
                   parameters,
                   (info, e) =>
                   {
                       if (e != null)
                       {
                           tcs.SetException(e);
                           return;
                       }

                       tcs.SetResult(info.Results);
                   });

            return tcs.Task;
        }

    }
}