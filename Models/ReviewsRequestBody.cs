namespace SteamReviesFetcher.Models;

public class ReviewsRequestBody
{
    public int Success { get; set; }
    public Query_Summary? Query_Summary { get; set; }
    public List<ReviewRequestBody> Reviews { get; set; } = [];
    public string Cursor { get; set; } = "*";
}