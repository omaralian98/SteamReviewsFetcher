using OfficeOpenXml;
using OfficeOpenXml.Table;
using System.IO;

namespace SteamReviesFetcher;
public static class ExcelExporter
{
    public static void ExportReviewsToExcel(List<ReviewToExcelData> reviews, string filePath)
    {
        if (reviews == null || reviews.Count == 0)
        {
            throw new ArgumentException("The reviews list is empty.");
        }

        var recommended = reviews.Where(x => x.IsRecommended).ToList();
        var notRecommended = reviews.Where(x => !x.IsRecommended).ToList();

        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using var package = new ExcelPackage();

        if (recommended.Count != 0 && notRecommended.Count != 0)
        {
            AddSheet(package, "All", reviews);
            AddSheet(package, "Positive", recommended);
            AddSheet(package, "Negative", notRecommended);
        }
        else if (recommended.Count != 0)
        {
            AddSheet(package, "Positive", recommended);
        }
        else if (notRecommended.Count != 0)
        {
            AddSheet(package, "Negative", notRecommended);
        }

        FileInfo file = new FileInfo(filePath);
        package.SaveAs(file);
    }

    private static void AddSheet(ExcelPackage package, string sheetName, List<ReviewToExcelData> reviews)
    {
        Dictionary<string, string> Languages = new Dictionary<string, string>
        {
            {"arabic", "Arabic"},
            {"bulgarian", "Bulgarian"},
            {"schinese", "Chinese (Simplified)"},
            {"tchinese", "Chinese (Traditional)"},
            {"czech", "Czech"},
            {"danish", "Danish"},
            {"dutch", "Dutch"},
            {"english", "English"},
            {"finnish", "Finnish"},
            {"french", "French"},
            {"german", "German"},
            {"greek", "Greek"},
            {"hungarian", "Hungarian"},
            {"indonesian", "Indonesian"},
            {"italian", "Italian"},
            {"japanese", "Japanese"},
            {"koreana", "Korean"},
            {"norwegian", "Norwegian"},
            {"polish", "Polish"},
            {"portuguese", "Portuguese"},
            {"brazilian", "Portuguese-Brazil"},
            {"romanian", "Romanian"},
            {"russian", "Russian"},
            {"spanish", "Spanish-Spain"},
            {"latam", "Spanish-Latin America"},
            {"swedish", "Swedish"},
            {"thai", "Thai"},
            {"turkish", "Turkish"},
            {"ukrainian", "Ukrainian"},
            {"vietnamese", "Vietnamese"}
        };

        var worksheet = package.Workbook.Worksheets.Add(sheetName);

        worksheet.Cells[1, 1].Value = "Steam ID";
        worksheet.Cells[1, 2].Value = "Profile URL";
        worksheet.Cells[1, 3].Value = "Language";
        worksheet.Cells[1, 4].Value = "Play Time (At Review in Hours)";
        worksheet.Cells[1, 5].Value = "Play Time (Forever in Hours)";
        worksheet.Cells[1, 6].Value = "Play Time (Last 2 Weeks in Hours)";
        worksheet.Cells[1, 7].Value = "Posted Date";
        worksheet.Cells[1, 8].Value = "Recommended";
        worksheet.Cells[1, 9].Value = "Votes Up";
        worksheet.Cells[1, 10].Value = "Review Length";
        worksheet.Cells[1, 11].Value = "Review Text";

        worksheet.View.FreezePanes(2, 1);

        for (int i = 0; i < reviews.Count; i++)
        {
            var review = reviews[i];
            worksheet.Cells[i + 2, 1].Value = review.SteamId;
            worksheet.Cells[i + 2, 2].Value = review.ProfileUrl;
            worksheet.Cells[i + 2, 3].Value = Languages.TryGetValue(review.Language, out var lang) ? lang : review.Language;
            worksheet.Cells[i + 2, 4].Value = review.PlayTimeAtReviewInHours;
            worksheet.Cells[i + 2, 5].Value = review.PlayTimeForeverInHours;
            worksheet.Cells[i + 2, 6].Value = review.PlayTimeLastTwoWeeksInHours;
            worksheet.Cells[i + 2, 7].Value = review.PostedDate.ToString("yyyy-MM-dd HH:mm:ss");
            worksheet.Cells[i + 2, 8].Value = review.IsRecommended;
            worksheet.Cells[i + 2, 9].Value = review.VotesUp;
            worksheet.Cells[i + 2, 10].Value = review.ReviewLength;
            worksheet.Cells[i + 2, 11].Value = review.ReviewText;
        }

        var range = worksheet.Cells[1, 1, reviews.Count + 1, 11];
        var table = worksheet.Tables.Add(range, $"{sheetName}Table");
        table.TableStyle = TableStyles.Medium2;

        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

        worksheet.Column(11).Width = 150;
        worksheet.Cells[range.Address].Style.WrapText = true;
    }
}