namespace ArcOrganization.Settings
{
    public interface IProtectData
    {
        byte[] Protect(byte[] userData, byte[] optionalEntropy);
        byte[] Unprotect(byte[] encryptedData, byte[] optionalEntropy);
    }
}
