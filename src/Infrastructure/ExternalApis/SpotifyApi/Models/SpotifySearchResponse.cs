public class SpotifySearchResponse
{
    public SpotifyTrackList? Tracks { get; set; }
}
public class SpotifyTrackList
{
    public List<SpotifyTrack>? Items { get; set; }
}
public class SpotifyTrack
{
    public string? Name { get; set; }
    public string? ArtistName { get; set; }
}