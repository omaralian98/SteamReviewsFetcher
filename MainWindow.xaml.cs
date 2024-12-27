using Microsoft.Win32;
using Newtonsoft.Json;
using SteamReviesFetcher;
using SteamReviesFetcher.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using System.Threading;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Kareem;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window, INotifyPropertyChanged
{
    private long _total = 0;
    public long Total
    {
        get => _total;
        set
        {
            _total = value;
        }
    }

    private long _max = long.MaxValue;
    public long Max
    {
        get => _max;
        set
        {
            _max = value;
        }
    }

    private bool _positive = true;

    public bool Positive
    {
        get => _positive;
        set
        {
            _positive = value;
        }
    }

    private bool _negative = true;

    public bool Negative
    {
        get => _negative;
        set
        {
            _negative = value;
        }
    }


    private Filter _filter = Filter.All;
    public Filter Filter
    {
        get => _filter;
        set
        {
            if (_filter != value)
            {
                _filter = value;
                OnPropertyChanged(nameof(Filter));
            }
        }
    }

    private PurchaseType _purchaseType;
    public PurchaseType PurchaseType
    {
        get => _purchaseType;
        set
        {
            if (_purchaseType != value)
            {
                _purchaseType = value;
                OnPropertyChanged(nameof(PurchaseType));
            }
        }
    }

    public List<ReviewToExcelData> reviewToExcelDatas { get; set; } = [];
    public CancellationTokenSource CancellationTokenSource = new();

    public ObservableCollection<KeyValuePair<string, string>> Languages { get; set; }


    public MainWindow()
    {
        InitializeComponent();
        Languages = new ObservableCollection<KeyValuePair<string, string>>
{
    new KeyValuePair<string, string>("Arabic", "arabic"),
    new KeyValuePair<string, string>("Bulgarian", "bulgarian"),
    new KeyValuePair<string, string>("Chinese (Simplified)", "schinese"),
    new KeyValuePair<string, string>("Chinese (Traditional)", "tchinese"),
    new KeyValuePair<string, string>("Czech", "czech"),
    new KeyValuePair<string, string>("Danish", "danish"),
    new KeyValuePair<string, string>("Dutch", "dutch"),
    new KeyValuePair<string, string>("English", "english"),
    new KeyValuePair<string, string>("Finnish", "finnish"),
    new KeyValuePair<string, string>("French", "french"),
    new KeyValuePair<string, string>("German", "german"),
    new KeyValuePair<string, string>("Greek", "greek"),
    new KeyValuePair<string, string>("Hungarian", "hungarian"),
    new KeyValuePair<string, string>("Indonesian", "indonesian"),
    new KeyValuePair<string, string>("Italian", "italian"),
    new KeyValuePair<string, string>("Japanese", "japanese"),
    new KeyValuePair<string, string>("Korean", "koreana"),
    new KeyValuePair<string, string>("Norwegian", "norwegian"),
    new KeyValuePair<string, string>("Polish", "polish"),
    new KeyValuePair<string, string>("Portuguese", "portuguese"),
    new KeyValuePair<string, string>("Portuguese-Brazil", "brazilian"),
    new KeyValuePair<string, string>("Romanian", "romanian"),
    new KeyValuePair<string, string>("Russian", "russian"),
    new KeyValuePair<string, string>("Spanish-Spain", "spanish"),
    new KeyValuePair<string, string>("Spanish-Latin America", "latam"),
    new KeyValuePair<string, string>("Swedish", "swedish"),
    new KeyValuePair<string, string>("Thai", "thai"),
    new KeyValuePair<string, string>("Turkish", "turkish"),
    new KeyValuePair<string, string>("Ukrainian", "ukrainian"),
    new KeyValuePair<string, string>("Vietnamese", "vietnamese")
};

        DataContext = this;
        Max = 2000;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }


    private void Button_Click(object sender, RoutedEventArgs e)
    {
        if (int.TryParse(AppIdTextBox.Text, out int appId))
        {

            reviewToExcelDatas.Clear();
            Export.Visibility = Visibility.Hidden;
            Stop.Visibility = Visibility.Visible;
            Fetch.IsEnabled = false;
            CancellationTokenSource = new();
            _ = GetAllReviews(appId, CancellationTokenSource);
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        CancellationTokenSource.Cancel();
    }

    private string[] GetSelectedLanguages()
    {
        return LanguagesListBox.SelectedItems.Cast<KeyValuePair<string, string>>()
                                             .Select(lang => lang.Value)
                                             .ToArray();
    }

    public async Task GetAllReviews(int appId, CancellationTokenSource? cancellationTokenSource = default)
    {

        string[] languages = GetSelectedLanguages();
        Filter filter = Filter;
        PurchaseType purchaseType = PurchaseType;
        string cursor = "*";
        ReviewsRequestBody? reviews = await GetReviews(appId, cursor, purchase_Type: purchaseType, languages: languages, ct: cancellationTokenSource?.Token);
        Total = Math.Min(reviews.Query_Summary.Total_Reviews, Max);
        do
        {
            if (cancellationTokenSource is not null && cancellationTokenSource.IsCancellationRequested)
            {
                MessageBox.Show("Fetching Was Stopped", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                break;
            }

            reviews = await GetReviews(appId, cursor, filter, purchaseType, languages, cancellationTokenSource?.Token);
            if (reviews is null || reviews.Success != 1)
            {
                break;
            }

            cursor = reviews.Cursor;
            reviewToExcelDatas.AddRange(reviews.Reviews.Select(x => new ReviewToExcelData(x)));
            Lab.Content = $"{reviewToExcelDatas.Count}/{Total}";

        } while (reviewToExcelDatas.Count < Total);
        if (reviewToExcelDatas.Count >= Total)
        {
            MessageBox.Show("Reviews Fetched successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        Stop.Visibility = Visibility.Hidden;
        Export.Visibility = Visibility.Visible;
        Fetch.IsEnabled = true;
    }

    public async Task<ReviewsRequestBody?> GetReviews(int appId, string cursor = "*", Filter filter = Filter.All, PurchaseType purchase_Type = PurchaseType.All, string[]? languages = default, CancellationToken? ct = default)
    {
        HttpClient client = new HttpClient();
        ReviewsRequestBody? reviews = null;
        CancellationToken cancellationToken = ct ?? new();
        try
        {
            string languageStr = languages is null ? "&language=english" : $"&language={string.Join('+', languages)}";
            string filterStr = filter == Filter.All ? "" : filter == Filter.Positive ? $"&review_type={filter.ToString().ToLower()}" : $"&review_type={filter.ToString().ToLower()}";
            string purchaseStr = purchase_Type == PurchaseType.All ? "" : purchase_Type == PurchaseType.Steam ? $"&purchase_type={purchase_Type.ToString().ToLower()}" : $"&purchase_type={purchase_Type.ToString().ToLower()}";
            string request = @$"https://store.steampowered.com/appreviews/{appId}?json=1&cursor={HttpUtility.UrlEncode(cursor)}&num_per_page=100{filterStr}{languageStr}";
            var json = await client.GetStringAsync(request, cancellationToken);
            reviews = JsonConvert.DeserializeObject<ReviewsRequestBody>(json, new UnixTimestampConverter());
        }
        catch (TaskCanceledException ex)
        {
            MessageBox.Show("Fetching Was Stopped", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"An error occurred while exporting the file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        return reviews;
    }

    private void Export_Click(object sender, RoutedEventArgs e)
    {
        var saveFileDialog = new SaveFileDialog
        {
            DefaultExt = ".xlsx",
            Filter = "Excel Files (*.xlsx)|*.xlsx",
            FileName = $"{AppIdTextBox.Text}Reviews.xlsx"
        };

        if (saveFileDialog.ShowDialog() == true)
        {
            string filePath = saveFileDialog.FileName;

            try
            {
                ExcelExporter.ExportReviewsToExcel(reviewToExcelDatas, filePath);

                MessageBox.Show("Excel file exported successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while exporting the file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

public enum Filter
{
    All,
    Positive,
    Negative
}

public enum PurchaseType
{
    All,
    Steam,
    Non_Steam_Purchase
}

public class EnumToBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null || parameter == null) return false;

        string enumValue = value.ToString();
        string parameterValue = parameter.ToString();

        return enumValue.Equals(parameterValue, StringComparison.InvariantCultureIgnoreCase);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isChecked && isChecked && parameter != null)
        {
            return Enum.Parse(targetType, parameter.ToString());
        }
        return Binding.DoNothing;
    }
}