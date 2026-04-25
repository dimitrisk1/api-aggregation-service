namespace Application.Interfaces
{
    public interface ICacheService
    {
        bool TryGetValue<T>(string key, out T? value);
        void Set<T>(string key, T value, TimeSpan absoluteExpirationRelativeToNow);
        Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory);
    }
}
