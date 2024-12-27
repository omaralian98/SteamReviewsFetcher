namespace SteamReviesFetcher.Models;

public class Query_Summary
{
    public int Num_Reviews { get; set; }
    public int Review_Score { get; set; }
    public string Review_Score_Desc { get; set; } = string.Empty;
    public int Total_Positive { get; set; }
    public int Total_Negative { get; set; }
    public int Total_Reviews { get; set; }
}