namespace ArcOrganization
{
    using System;
    using System.Threading.Tasks;
    using System.Windows.Controls;

    using ArcOrganization.Hub;
    using ArcOrganization.Infrastructure.ViewModels;
    using ArcOrganization.Services;

    using Caliburn.Micro;

    public class LoginViewModel : ScreenViewModel
    {
        private readonly INavigationService _navigationService;

        private readonly PortalService _portalService;

        private PasswordBox _passwordBox;

        private string _username;

        private bool _canLogin;

        private string _errorMessage;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoginViewModel"/> class. 
        /// </summary>
        /// <param name="navigationService">
        /// The navigation service.
        /// </param>
        /// <param name="portalService">
        /// The portal service.
        /// </param>
        public LoginViewModel(INavigationService navigationService, PortalService portalService)
        {
            _navigationService = navigationService;
            _portalService = portalService;

            // Set default values
            Username = "kajanus_dev";
            CanLogin = true;
        }

        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        public string Username
        {
            get
            {
                return _username;
            }

            set
            {
                if (value.Equals(_username))
                {
                    return;
                }
                _username = value;
                NotifyOfPropertyChange(() => Username);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the user can execute Login process.
        /// </summary>
        public bool CanLogin
        {
            get
            {
                return _canLogin;
            }

            set
            {
                if (value.Equals(_canLogin))
                {
                    return;
                }
                _canLogin = value;
                NotifyOfPropertyChange(() => CanLogin);
            }
        }

        /// <summary>
        /// Gets or sets the error message for the UI.
        /// </summary>
        public string ErrorMessage
        {
            get
            {
                return _errorMessage;
            }

            set
            {
                if (value.Equals(_errorMessage))
                {
                    return;
                }
                _errorMessage = value;
                NotifyOfPropertyChange(() => ErrorMessage);
            }
        }

        /// <summary>
        /// Logs user into the application with given credentials.
        /// </summary>
        /// <param name="password"></param>
        public async void Login(string password)
        {
            SetBusy("Logining...");
            ErrorMessage = string.Empty;
            CanLogin = false;

            await LoginAndNavigateAsync();
        }

        private async Task LoginAndNavigateAsync()
        {
            try
            {
                // Generates token for the user and then naviates to Map view.
                await _portalService.InitializeAsync(Username, _passwordBox.Password);
                _navigationService.UriFor<HubViewModel>().WithParam(p => p.RemoveFromBackstackNavigation, true).Navigate();
                SetNotBusy();
            }
            catch (Exception exception)
            {
                ErrorMessage = exception.Message;
                CanLogin = true;
                SetNotBusy();
            }
        }

        /// <summary>
        /// Executed when the view is loaded. This violates the MVVM pattern, but is used to keep password secured.
        /// </summary>
        /// <param name="view">The view.</param>
        protected override void OnViewLoaded(object view)
        {
            _passwordBox = (view as LoginView).PasswordBox;
            _passwordBox.Password = "PreviousCheckInIsChangedForNow:)";
            base.OnViewLoaded(view);
        }
    }
}