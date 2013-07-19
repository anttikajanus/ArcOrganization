namespace ArcOrganization.Infrastructure
{
    using System.Threading.Tasks;

    public interface IRefreshable
    {
        Task RefreshContentAsync();
    }
}
