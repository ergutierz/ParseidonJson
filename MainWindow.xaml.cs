using System.Windows;
using ParseidonJson.parser;
using ParseidonJson.remote;

namespace ParseidonJson
{
    public partial class MainWindow : Window
    {
        private readonly SportsService _sportsService;
        private readonly IJsonToCSharpClassGenerator _jsonToCSharpClassGenerator;

        public MainWindow()
        {
            InitializeComponent();
        }
        
        public MainWindow(
            SportsService sportsService,
            IJsonToCSharpClassGenerator jsonToCSharpClassGenerator
        ) : this()
        {
            _sportsService = sportsService;
            _jsonToCSharpClassGenerator = jsonToCSharpClassGenerator;
        }

        private async void submitButton_Click(object sender, RoutedEventArgs e)
        {
            progressBar.Visibility = Visibility.Visible;
            try
            {
                string jsonContent = jsonInputBox.Text;
                var dataModel = await Task.Run(() => _jsonToCSharpClassGenerator.GenerateCSharpClasses(jsonContent));
                outputBox.Text = dataModel;
                elapsedTimeLabel.Content =
                    $"Elapsed Time for Processing: {_jsonToCSharpClassGenerator.LastOperationElapsedTimeMs:F2}ms";
            }
            catch (Exception ex)
            {
                jsonInputBox.Text = string.Empty;
                outputBox.Text = $"Failed to convert JSON data: {ex.Message}";
            }
            finally
            {
                progressBar.Visibility = Visibility.Collapsed;
            }
        }

        private void clearButton_Click(object sender, RoutedEventArgs e)
        {
            jsonInputBox.Text = string.Empty;
            outputBox.Text = string.Empty;
            elapsedTimeLabel.Content = string.Empty;
        }

        protected override void OnClosed(EventArgs e)
        {
            _sportsService.Dispose();
            base.OnClosed(e);
        }

        private async void FetchSportsStatsButton_OnClickButton_Click(object sender, RoutedEventArgs e)
        {
            progressBar.Visibility = Visibility.Visible;

            string url = "http://sports.snoozle.net/search/nfl/searchHandler?fileType=inline&statType=teamStats&season=2020&teamName=26";
            string jsonContent = string.Empty;

            try
            {
                jsonContent = await Task.Run(() => _sportsService.FetchSportsStatsAsync(url).Result);

                var dataModel = await Task.Run(() => _jsonToCSharpClassGenerator.GenerateCSharpClasses(jsonContent));

                jsonInputBox.Text = jsonContent;
                outputBox.Text = dataModel;
                elapsedTimeLabel.Content =  $"Elapsed Time for Processing: {_jsonToCSharpClassGenerator.LastOperationElapsedTimeMs:F2}ms";
            }
            catch (Exception ex)
            {
                jsonInputBox.Text = string.Empty;
                outputBox.Text = $"Failed to fetch JSON data: {ex.Message}";
            }
            finally
            {
                progressBar.Visibility = Visibility.Collapsed;
            }
        }

        private void CopyModelButton_OnClickButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(outputBox.Text))
            {
                Clipboard.SetText(outputBox.Text);
                MessageBox.Show("Content copied to clipboard!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}