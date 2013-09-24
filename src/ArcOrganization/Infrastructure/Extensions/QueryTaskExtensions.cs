namespace ArcOrganization.Infrastructure.Extensions
{
    using System;
    using System.Threading.Tasks;

    using ESRI.ArcGIS.Client.Tasks;

    /// <summary>
    ///     Extensions to provide Task based methods for <see cref="QueryTask"/>. 
    /// </summary>
    public static class QueryTaskExtensions
    {
        /// <summary>
        /// Executes query.
        /// </summary>
        /// <param name="task">Query task execute.</param>
        /// <param name="query">Query parameters.</param>
        /// <returns>Returns <see cref="QueryEventArgs"/> when query is completed.</returns>
        public static async Task<QueryEventArgs> ExecuteTaskAsync(this QueryTask task, Query query)
        {
            var tcs = new TaskCompletionSource<QueryEventArgs>();

            EventHandler<QueryEventArgs> completed = (s, e) => { tcs.SetResult(e); };
            EventHandler<TaskFailedEventArgs> failed = (s, e) => { tcs.SetException(e.Error); };

            QueryEventArgs result;

            try
            {
                task.ExecuteCompleted += completed;
                task.Failed += failed;

                task.ExecuteAsync(query);
                result = await tcs.Task;
            }
            finally
            {
                task.ExecuteCompleted -= completed;
                task.Failed -= failed;
            }

            return result;
        }
    }
}
