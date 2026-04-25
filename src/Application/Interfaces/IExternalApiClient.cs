namespace Application.Interfaces
{
    using Domain.Entities;
    using Application.DTOs;

    public interface IExternalApiClient
    {
        string ProviderName { get; }

        // T represents the unified shape or the raw shape we will map to UnifiedItem
        Task<ProviderResult<T>> FetchDataAsync<T>(string query, CancellationToken cancellationToken);
    }
}
