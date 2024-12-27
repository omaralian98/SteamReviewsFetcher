namespace SteamReviesFetcher.Models;

public class ReviewAuthor
{
    public string SteamId { get; set; }
    public int Num_Games_Owned { get; set; }
    public int Num_Reviews { get; set; }
    public long PlayTime_Forever { get; set; }
    public long PlayTime_Last_Two_Weeks { get; set; }
    public long PlayTime_At_Review { get; set; }
    public DateTime Last_Played { get; set; }
}