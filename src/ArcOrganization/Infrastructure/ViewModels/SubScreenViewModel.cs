namespace ArcOrganization.Infrastructure.ViewModels
{
    using System.Threading.Tasks;

    using Caliburn.Micro;

    /// <summary>
    /// The base ViewModel that derives from the <see cref="Screen"/>. 
    /// </summary>
    /// <remarks>
    /// Use this base class to provide Screen behavior for the ViewModel</remarks>
    public abstract class SubScreenViewModel : Screen, IRefreshable
    {
        private bool _isBusy;

        private string _busyText;

        private bool _isLoaded;

        /// <summary>
        /// Gets or sets a value indicating whether the ViewModel is busy like initializing or searching.
        /// </summary>
        public bool IsBusy
        {
            get
            {
                return _isBusy;
            }

            set
            {
                if (value.Equals(_isBusy))
                {
                    return;
                }

                _isBusy = value;
                NotifyOfPropertyChange(() => IsBusy);
            }
        }

        /// <summary>
        /// Gets or sets the text for the busy indication
        /// </summary>
        public string BusyText
        {
            get
            {
                return _busyText;
            }

            set
            {
                if (value == _busyText)
                {
                    return;
                }

                _busyText = value;
                NotifyOfPropertyChange(() => BusyText);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the ViewModel has loaded its initial content.
        /// </summary>
        public bool IsLoaded
        {
            get
            {
                return _isLoaded;
            }

            set
            {
                if (value.Equals(_isLoaded))
                {
                    return;
                }
                _isLoaded = value;
                NotifyOfPropertyChange(() => IsLoaded);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether previous page is removed the source page from the back stack.
        /// </summary>
        /// <remarks>
        /// Usage :  
        /// <code>
        /// this.navigationService.UriFor{ViewModelWhereToNavigate}().WithParam(p => p.RemoveFromBackstackNavigation, true).Navigate();
        /// </code>
        /// </remarks>
        public bool RemoveFromBackstackNavigation { get; set; }

        protected virtual void SetBusy(string busyText = "Loading...")
        {
            BusyText = busyText;
            IsBusy = true;
        }

        protected virtual void SetNotBusy()
        {
            IsBusy = false;
            BusyText = string.Empty;
        }

        /// <summary>
        /// Override to load initial content.
        /// </summary>
        /// <returns></returns>
        public virtual async Task LoadContentAsync()
        {
        }

        public virtual async Task RefreshContentAsync()
        {
            await  LoadContentAsync();
        }
    }
}
