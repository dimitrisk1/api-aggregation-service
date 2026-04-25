namespace Core_Infrastructure.ExternalApis.GithubApi.Models
{
    public class GithubSearchResponse
    {
        public List<GithubRepo>? Items { get; set; }
    }
    public class GithubRepo
    {
        public string? FullName { get; set; }
        public string? Description { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

}
