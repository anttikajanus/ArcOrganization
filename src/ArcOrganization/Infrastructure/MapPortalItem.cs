namespace ArcOrganization.Infrastructure
{
    using Caliburn.Micro;

    using ESRI.ArcGIS.Client.Portal;

    public class MapPortalItem : PropertyChangedBase 
    {
        private ArcGISPortalItem _item;

        private bool _isOnMap;

        public ArcGISPortalItem Item
        {
            get
            {
                return _item;
            }
            set
            {
                if (Equals(value, _item))
                {
                    return;
                }
                _item = value;
                NotifyOfPropertyChange(() => Item);
            }
        }

        public bool IsOnMap
        {
            get
            {
                return _isOnMap;
            }
            set
            {
                if (value.Equals(_isOnMap))
                {
                    return;
                }
                _isOnMap = value;
                NotifyOfPropertyChange(() => IsOnMap);
            }
        }
    }
}
