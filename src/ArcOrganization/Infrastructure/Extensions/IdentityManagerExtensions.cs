namespace ArcOrganization.Infrastructure.Extensions
{
    using System.Threading.Tasks;

    using ESRI.ArcGIS.Client;

    public static class IdentityManagerExtensions
    {
        /// <summary>
        /// Generates credentials for the user to the ArcGIS Online.
        /// </summary>
        /// <param name="identityManager">IdentityManager used.</param>
        /// <param name="username">The Username.</param>
        /// <param name="password">The password.</param>
        /// <param name="tokenValidity">Token validity time in minutes</param>
        /// <param name="referer">The referer</param>
        /// <returns>Returns <see cref="IdentityManager.Credential"/> as a <see cref="Task"/></returns>
        public static Task<IdentityManager.Credential> GetCredentialsAsync(this IdentityManager identityManager, string username, string password, string referer, int tokenValidity = 60)
        {
            var tcs = new TaskCompletionSource<IdentityManager.Credential>();

            IdentityManager.Current.TokenValidity = tokenValidity;
            identityManager.TokenGenerationReferer = referer;
            IdentityManager.Current.ChallengeMethod += (url, handler, options) => IdentityManager.Current.GenerateCredentialAsync(
                url,
                username,
                password,
                handler,
                options);

            IdentityManager.Current.GetCredentialAsync(
                UrlResources.ArcGISPortalUrl,
                true,
                (credential, exception) =>
                {
                    if (exception != null)
                    {
                        tcs.SetException(exception);
                        return;
                    }

                    credential.AutoRefresh = true;
                    tcs.SetResult(credential);
                });

            return tcs.Task;
        }
    }
}
