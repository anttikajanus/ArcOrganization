namespace ArcOrganization.Infrastructure.ViewModels
{
    using ESRI.ArcGIS.Client;
    using ESRI.ArcGIS.Client.Geometry;

    public abstract class MapScreenViewModel : ScreenViewModel
    {
        private LayerCollection _layers;
        private Envelope _extent;

        protected MapScreenViewModel()
        {
             Layers = new LayerCollection();
        }

        /// <summary>
        /// Gets or sets the extent of the Map.
        /// </summary>
        public Envelope Extent
        {
            get
            {
                return  _extent;
            }
            set
            {
                if (Equals(value,  _extent))
                {
                    return;
                }
                 _extent = value;
                 NotifyOfPropertyChange(() =>  Extent);
            }
        }

        /// <summary>
        /// Get the layers used in the Map.
        /// </summary>
        public LayerCollection Layers
        {
            get
            {
                return  _layers;
            }
            
            set
            {
                if (Equals(value,  _layers))
                {
                    return;
                }
                 _layers = value;
                 NotifyOfPropertyChange(() =>  Layers);
            }
        }     
    }
}
