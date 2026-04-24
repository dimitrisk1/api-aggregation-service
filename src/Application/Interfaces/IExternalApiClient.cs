namespace Application.Interfaces
{
    using Domain.Entities;
    using Application.DTOs;

    public interface IExternalApiClient
    {
        string Name { get; }
        Task<IEnumerable<UnifiedItem>> FetchAsync(AggregationRequest request);
    }
}
