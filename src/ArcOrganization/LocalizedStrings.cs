namespace ArcOrganization
{
    using ArcOrganization.Resources;

    /// <summary>
    /// Provides access to string resources.
    /// </summary>
    public class LocalizedStrings
    {
        private static readonly AppResources appResources = new AppResources();
        private static readonly TextResources localizedResources = new TextResources();

        /// <summary>
        /// Gets the text resources.
        /// </summary>
        public TextResources LocalizedResources
        {
            get { return localizedResources; }
        }

        /// <summary>
        /// Gets the application resources.
        /// </summary>
        public AppResources AppResources
        {
            get { return appResources; }
        }
    }
}