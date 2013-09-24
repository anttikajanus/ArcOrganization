namespace ArcOrganization.Settings
{
    using System.Security.Cryptography;

    public class ProtectDataAdapter : IProtectData
    {
        public byte[] Protect(byte[] userData, byte[] optionalEntropy)
        {
            return ProtectedData.Protect(userData, optionalEntropy);
        }

        public byte[] Unprotect(byte[] encryptedData, byte[] optionalEntropy)
        {
            return ProtectedData.Unprotect(encryptedData, optionalEntropy);
        }
    }
}
