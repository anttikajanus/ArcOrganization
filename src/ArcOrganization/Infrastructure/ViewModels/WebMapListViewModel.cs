namespace ArcOrganization.Infrastructure.ViewModels
{
    using Caliburn.Micro;

    using ESRI.ArcGIS.Client.Portal;

    public abstract class WebMapListViewModel : SubScreenViewModel
    {
        private BindableCollection<ArcGISPortalItem> _items;

        private ArcGISPortalItem _selectedItem;

        public ArcGISPortalItem SelectedItem
        {
            get
            {
                return _selectedItem;
            }
            set
            {
                if (Equals(value, _selectedItem))
                {
                    return;
                }
                _selectedItem = value;
                WebMapChanged(_selectedItem);
                NotifyOfPropertyChange(() => SelectedItem);
            }
        }

        /// <summary>
        /// Gets or sets the new maps as a ArcGIS Portal Items 
        /// </summary>
        public BindableCollection<ArcGISPortalItem> Items
        {
            get
            {
                return _items;
            }

            set
            {
                if (Equals(value, _items))
                {
                    return;
                }
                _items = value;
                NotifyOfPropertyChange(() => Items);
            }
        }

        /// <summary>
        /// Override to implmement changed behavior
        /// </summary>
        /// <param name="portalItem"></param>
        protected virtual void WebMapChanged(ArcGISPortalItem portalItem)
        {
        }
    }
}