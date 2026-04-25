namespace Domain.Interfaces
{
    using Domain.Entities;
    using Domain.Models;

    public interface IExternalProvider
    {
        string ProviderName { get; }

        Task<ProviderResult<IEnumerable<UnifiedItem>>> FetchDataAsync(string query, CancellationToken cancellationToken);
    }
}
