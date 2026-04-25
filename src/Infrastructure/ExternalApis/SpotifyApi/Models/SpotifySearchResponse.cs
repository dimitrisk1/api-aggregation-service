namespace Infrastructure.ExternalApis.SpootifyApi
{
    public class SpotifySearchResponse
    {
        public TrackContainer Tracks { get; set; }
    }

    public class TrackContainer
    {
        public IList<TrackItem> Items { get; set; } = new List<TrackItem>();
    }

    public class TrackItem
    {
        public string Name { get; set; }
        public IList<Artist> Artists { get; set; } = new List<Artist>();
        public string PreviewUrl { get; set; }
    }

    public class Artist
    {
        public string Name { get; set; }
    }
}
