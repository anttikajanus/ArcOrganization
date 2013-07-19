namespace ArcOrganization
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Navigation;

    using Autofac;

    using Caliburn.Micro;
    using Caliburn.Micro.Autofac;

    using Microsoft.Phone.Controls;

    /// <summary>
    /// Bootstrapper for the application. This class constructs and hooks phone application and its services. 
    /// </summary>
    public class AppBootstrapper : AutofacBootstrapper
    {
        protected override void ConfigureBootstrapper()
        {
            base.ConfigureBootstrapper();
            EnforceNamespaceConvention = false;
        }

        protected override void ConfigureContainer(ContainerBuilder builder)
        {
            base.ConfigureContainer(builder);
            builder.RegisterAssemblyTypes(AssemblySource.Instance.ToArray())
                // Must be a type with a name that ends with Service.
                   .Where(aType => aType.Name.EndsWith("Service"))
                // Registered as self.
                   .AsSelf()
                // As singleton.
                   .SingleInstance();

            RootFrame.Navigated += RootFrameNavigated;
        }

        protected override void OnUnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
#warning TODO Exception handling
#if DEBUG

            MessageBox.Show(e.ExceptionObject.ToString());
            Debug.WriteLine("Error : {0}", e.ExceptionObject);

#endif
        }

        private void RootFrameNavigated(object sender, NavigationEventArgs e)
        {
            // Remove previous page (Where navigation was done) from backstack.
            if (e.NavigationMode == NavigationMode.New && e.Uri.ToString().Contains("RemoveFromBackstackNavigation=True"))
            {
                RootFrame.RemoveBackEntry();
            }
        }
    }
}