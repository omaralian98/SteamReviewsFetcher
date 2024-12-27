namespace SteamReviesFetcher.Models;

public class ReviewRequestBody
{
    public string RecommendationId { get; set; }
    public ReviewAuthor Author { get; set; }
    public string Language { get; set; }
    public string Review { get; set; }
    public DateTime TimeStamp_Created { get; set; }
    public DateTime TimeStamp_Updated { get; set; }
    public bool Voted_Up { get; set; }
    public int Votes_Up { get; set; }
    public int Votes_Funny { get; set; }
    public double Weighted_Vote_Score { get; set; }
    public int Comment_Count { get; set; }
    public bool Steam_Purchase { get; set; }
    public bool Received_For_Free { get; set; }
    public bool Written_During_Early_Access { get; set; }
    public bool Primarily_Steam_Deck { get; set; }
}