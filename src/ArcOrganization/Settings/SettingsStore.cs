namespace ArcOrganization.Settings
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.IO.IsolatedStorage;

    using ESRI.ArcGIS.Client.Portal;

    public class SettingsStore : ISettingsStore
    {
        private readonly IProtectData _protectDataAdapter;

        private readonly IsolatedStorageSettings _isolatedStore;
        private UTF8Encoding _encoding;

        private const string PasswordSettingKeyName = "PasswordSetting";
        private const string PasswordSettingDefault = "";

        private const string UserNameSettingKeyName = "UserNameSetting";
        private const string UserNameSettingDefault = "kajanus_dev";

        private const string RememberMeSettingKeyName = "RememberMeSetting";
        private const bool RememberMeSettingDefault = true;

        private const string LocationServiceAllowedKeyName = "LocationserviceAllowedSetting";
        private const bool LocationServiceAllowedDefault = true;

        private const string BasemapItemsCacheKeyName = "BasemapItemsCache";
        private const string BasemapItemsSavedCacheKeyName = "BasemapItemsSavedCache";
        private List<ArcGISPortalItem> BasemapItemsSettingDefault = new List<ArcGISPortalItem>(); 

        private const string NewestOrganizationWebMapsItemsCacheKeyName = "NewestOrganizationWebMapsItemsCache";
        private const string NewestOrganizationWebMapsItemsSavedCacheKeyName = "NewestOrganizationWebMapsItemsSavedCache";
        private List<ArcGISPortalItem> NewestOrganizationWebMapsItemsSettingDefault = new List<ArcGISPortalItem>(); 

        public SettingsStore(IProtectData protectDataAdapter)
        {
            _protectDataAdapter = protectDataAdapter;
            _isolatedStore = IsolatedStorageSettings.ApplicationSettings;
            _encoding = new UTF8Encoding();
        }

        public string Password
        {
            get
            {
                return PasswordByteArray.Length == 0
                           ? PasswordSettingDefault
                           : _encoding.GetString(PasswordByteArray, 0, PasswordByteArray.Length);
            }
            set
            {
                PasswordByteArray = _encoding.GetBytes(value);
            }
        }

        public string UserName 
        {
            get
            {
                return GetValueOrDefault(UserNameSettingKeyName, UserNameSettingDefault);
            }
            set
            {
                AddOrUpdateValue(UserNameSettingKeyName, value);
            }
        }

        public bool LocationServiceAllowed 
        {
            get
            {
                return GetValueOrDefault(LocationServiceAllowedKeyName, LocationServiceAllowedDefault);
            }
            set
            {
                AddOrUpdateValue(LocationServiceAllowedKeyName, value);
            }
        }

        public bool RememberMe
        {
            get
            {
                return GetValueOrDefault(RememberMeSettingKeyName, RememberMeSettingDefault);
            }
            set
            {
                AddOrUpdateValue(RememberMeSettingKeyName, value);
            }
        }

        public List<ArcGISPortalItem> BasemapItems
        {
            get
            {
                return GetValueOrDefault(BasemapItemsCacheKeyName, BasemapItemsSettingDefault);
            }
            set
            {
                AddOrUpdateValue(BasemapItemsCacheKeyName, value);
            }
        }

        public DateTime BasemapItemsSaved
        {
            get
            {
                return GetValueOrDefault(BasemapItemsSavedCacheKeyName, DateTime.MinValue);
            }
            set
            {
                AddOrUpdateValue(BasemapItemsSavedCacheKeyName, value);
            }
        }

        public List<ArcGISPortalItem> NewestOrganizationWebMapItems
        {
            get
            {
                return GetValueOrDefault(NewestOrganizationWebMapsItemsCacheKeyName, NewestOrganizationWebMapsItemsSettingDefault);
            }
            set
            {
                AddOrUpdateValue(NewestOrganizationWebMapsItemsCacheKeyName, value);
            }
        }

        public DateTime NewestOrganizationWebMapsSaved
        {
            get
            {
                return GetValueOrDefault(NewestOrganizationWebMapsItemsSavedCacheKeyName, DateTime.MinValue);
            }
            set
            {
                AddOrUpdateValue(NewestOrganizationWebMapsItemsSavedCacheKeyName, value);
            }

        }

        private byte[] PasswordByteArray
        {
            get
            {
                byte[] encryptedValue = GetValueOrDefault(PasswordSettingKeyName, new byte[0]);
                if (encryptedValue.Length == 0)
                {
                    return new byte[0];
                }
                return _protectDataAdapter.Unprotect(encryptedValue, null);
            }
            set
            {
                byte[] encryptedValue = _protectDataAdapter.Protect(value, null);
                AddOrUpdateValue(PasswordSettingKeyName, encryptedValue);
            }
        }

        private void AddOrUpdateValue(string key, object value)
        {
            bool valueChanged = false;

            try
            {
                // If the new value is different, set the new value.
                if (_isolatedStore[key] != value)
                {
                    _isolatedStore[key] = value;
                    valueChanged = true;
                }
            }
            catch (KeyNotFoundException)
            {
                _isolatedStore.Add(key, value);
                valueChanged = true;
            }
            catch (ArgumentException)
            {
                _isolatedStore.Add(key, value);
                valueChanged = true;
            }

            if (valueChanged)
            {
                Save();
            }
        }

        private T GetValueOrDefault<T>(string key, T defaultValue)
        {
            T value;

            try
            {
                value = (T)_isolatedStore[key];
            }
            catch (KeyNotFoundException)
            {
                value = defaultValue;
            }
            catch (ArgumentException)
            {
                value = defaultValue;
            }

            return value;
        }

        private void Save()
        {
            _isolatedStore.Save();
        }
    }
}