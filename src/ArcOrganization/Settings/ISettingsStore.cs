namespace ArcOrganization.Settings
{
    using System;
    using System.Collections.Generic;

    using ESRI.ArcGIS.Client.Portal;

    public interface ISettingsStore
    {
        string Password { get; set; }
        string UserName { get; set; }
        bool RememberMe { get; set; }
        bool LocationServiceAllowed { get; set; }

        List<ArcGISPortalItem> BasemapItems { get; set; }
        DateTime BasemapItemsSaved { get; set; }

        List<ArcGISPortalItem> NewestOrganizationWebMapItems { get; set; }
        DateTime NewestOrganizationWebMapsSaved { get; set; }
    }
}
