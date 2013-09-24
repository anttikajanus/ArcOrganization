namespace ArcOrganization.Infrastructure.ViewModels
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using Caliburn.Micro;

    /// <summary>
    /// The base ViewModel that derives from the <see cref="Conductor{SubSreenViewModel}"/>. 
    /// </summary>
    /// <remarks>
    /// Use this base class to provide conductors behavior for the ViewModel</remarks>
    public abstract class ConductorViewModel<T> : Conductor<T>.Collection.AllActive
        where T : SubScreenViewModel
    {
        private bool _isBusy;

        private string _busyText;

        private bool _isLoaded;

        private DateTime _contentLoaded;

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
        /// Gets if the content can be refreshed.
        /// </summary>
        public bool CanRefreshContent
        {
            get
            {
                return true;
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

        /// <summary>
        /// Raised when the view model is activated
        /// </summary>
        protected override async void OnActivate()
        {
            if (!IsLoaded)
            {
                SetBusy();
                
                //await Task.Run(async () =>
                //{
                    await LoadAllContentAsync();
                IsLoaded = true;
                _contentLoaded = DateTime.Now;   
                //}).ContinueWith(t => 
                //    {
                        SetNotBusy();
//                    }, TaskScheduler.FromCurrentSynchronizationContext()); 
            }
            else
            {
                var refeshTime = DateTime.Now.AddMinutes(-10);
                if (_contentLoaded > refeshTime)
                {
                    return;
                }

                await LoadAllContentAsync();
                IsLoaded = true;
                _contentLoaded = DateTime.Now;
                SetNotBusy();
            }

            base.OnActivate();
        }

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

        public virtual async Task LoadAllContentAsync()
        {
            SetBusy("Loading content...");

            var loadContentOperations = (from item in Items select item.LoadContentAsync()).ToArray();
            try
            {
                await Task.WhenAll(loadContentOperations);
            }
            catch (Exception exc)
            {
                foreach (Task<string> faulted in loadContentOperations.Where(t => t.IsFaulted))
                {
                    // work with faulted and faulted.Exception
                    // TODO
                }
            }

            SetNotBusy();
        }

        //public virtual async Task RefreshActiveContentAsync()
        //{
            //var refreshableChild = ActiveItem as IRefreshable;
            //if (refreshableChild != null)
            //{
            //    SetBusy("Refreshing content...");
            //    await refreshableChild.RefreshContentAsync();
            //    SetNotBusy();
            //}
        //}
    }
}