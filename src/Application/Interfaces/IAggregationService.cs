namespace Application.Interfaces
{
    using Application.DTOs;

    public interface IAggregationService
    {
       public Task<AggregatedResponse> HandleAsync(AggregationRequest request);
    }
}
