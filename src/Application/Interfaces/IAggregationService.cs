namespace Application.Interfaces
{
    using Application.DTOs;

    public interface IAggregationService
    {
        Task<AggregatedResponse> AggregateDataAsync(AggregationRequest request, CancellationToken cancellationToken);
    }
}
