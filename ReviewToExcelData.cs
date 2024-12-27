using SteamReviesFetcher.Models;

namespace SteamReviesFetcher;

public class ReviewToExcelData(ReviewRequestBody review)
{
    public string SteamId { get; private set; } = review.Author.SteamId;
    public string ProfileUrl { get; private set; } = $"https://steamcommunity.com/profiles/{review.Author.SteamId}/";
    public string Language { get; private set; } = review.Language;

    public long PlayTimeAtReviewInHours { get; private set; } = Convert.ToInt32(review.Author.PlayTime_At_Review / 60);
    public long PlayTimeForeverInHours { get; private set; } = Convert.ToInt32(review.Author.PlayTime_Forever / 60);
    public long PlayTimeLastTwoWeeksInHours { get; private set; } = Convert.ToInt32(review.Author.PlayTime_Last_Two_Weeks / 60);

    public int VotesUp { get; set; } = review.Votes_Up;
    public DateTime PostedDate { get; private set; } = review.TimeStamp_Created;
    public bool IsRecommended { get; private set; } = review.Voted_Up;
    public int ReviewLength { get; private set; } = review.Review.Length;
    public string ReviewText { get; private set; } = review.Review;
}
