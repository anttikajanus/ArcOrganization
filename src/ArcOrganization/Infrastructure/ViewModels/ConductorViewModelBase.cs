namespace ArcOrganization.Infrastructure.ViewModels
{
    using Caliburn.Micro;

    /// <summary>
    /// The base ViewModel that derives from the <see cref="Conductor{T}"/>. 
    /// </summary>
    /// <typeparam name="T">
    /// Type of the conducted item.
    /// </typeparam>
    /// <remarks>
    /// Use this base class to provide conductors behavior for the ViewModel</remarks>
    public abstract class ConductorViewModel<T> : Conductor<T>
        where T : class
    {
        #region Variables
     
        private bool isBusy;
        private string busyText;

        #endregion // Variables

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether the ViewModel is busy like initializing or searching.
        /// </summary>
        public bool IsBusy
        {
            get
            {
                return  isBusy;
            }

            set
            {
                if (value.Equals( isBusy))
                {
                    return;
                }

                 isBusy = value;
                 NotifyOfPropertyChange(() =>  IsBusy);
            }
        }

        /// <summary>
        /// Gets or sets the text for the busy indication
        /// </summary>
        public string BusyText
        {
            get
            {
                return  busyText;
            }

            set
            {
                if (value ==  busyText)
                {
                    return;
                }

                 busyText = value;
                 NotifyOfPropertyChange(() =>  BusyText);
            }
        }

        #endregion
    }
}
