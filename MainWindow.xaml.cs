using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using ParseidonJson.core;
using ParseidonJson.remote;

namespace ParseidonJson
{
    public partial class MainWindow : Window
    {
        private readonly SportsService _sportsService;
        private readonly IJsonParser _jsonParser;
        private readonly IJsonQuery _jsonQuery;
        private const string Placeholder = "Paste JSON here (optional)";

        public MainWindow()
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            jsonDirectInput.Text = Placeholder;
            jsonDirectInput.Foreground = Brushes.Gray;
            jsonDirectInput.GotFocus += jsonDirectInput_GotFocus;
            jsonDirectInput.LostFocus += jsonDirectInput_LostFocus;
            ResetJsonDirectInput();
            ConfigureAvalonEdit();
        }

        public MainWindow(
            SportsService sportsService,
            IJsonParser jsonParser,
            IJsonQuery jsonQuery
        ) : this()
        {
            _sportsService = sportsService;
            _jsonParser = jsonParser;
            _jsonQuery = jsonQuery;
        }

        // Begin Region Json Parsing

        private async void submitButton_Click(object sender, RoutedEventArgs e)
        {
            progressBar.Visibility = Visibility.Visible;
            try
            {
                string jsonContent = jsonInputBox.Text;
                var dataModel = await Task.Run(() => _jsonParser.GenerateCSharpClasses(jsonContent));
                outputBox.Text = dataModel;
                elapsedTimeLabel.Content =
                    $"Elapsed Time for Processing: {_jsonParser.LastOperationElapsedTimeMs:F2}ms";
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

            string url =
                "http://sports.snoozle.net/search/nfl/searchHandler?fileType=inline&statType=teamStats&season=2020&teamName=26";
            string jsonContent = string.Empty;

            try
            {
                jsonContent = await Task.Run(() => _sportsService.FetchSportsStatsAsync(url).Result);

                var dataModel = await Task.Run(() => _jsonParser.GenerateCSharpClasses(jsonContent));

                jsonInputBox.Text = jsonContent;
                outputBox.Text = dataModel;
                elapsedTimeLabel.Content =
                    $"Elapsed Time for Processing: {_jsonParser.LastOperationElapsedTimeMs:F2}ms";
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
                MessageBox.Show("Content copied to clipboard!", "Success", MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        // End Region Json Parsing

        // Begin Region Editor

        private void NewJson_Click(object sender, RoutedEventArgs e)
        {
            jsonEditor.Text = "{}";
            messageArea.Text = "New JSON object ready to be edited.";
        }

        private void SaveJson_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*",
                DefaultExt = ".json",
                Title = "Save JSON"
            };

            bool? result = saveFileDialog.ShowDialog();

            if (result == true)
            {
                string filename = saveFileDialog.FileName;

                File.WriteAllText(filename, jsonEditor.Text);

                messageArea.Text = "JSON saved successfully.";
                messageArea.Foreground = new SolidColorBrush(Colors.Green);
            }
        }

        private void ValidateJson_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Newtonsoft.Json.Linq.JToken.Parse(jsonEditor.Text);
                messageArea.Text = "JSON is valid.";
                messageArea.Foreground = new SolidColorBrush(Colors.Green);
            }
            catch (Newtonsoft.Json.JsonReaderException ex)
            {
                messageArea.Text = $"Invalid JSON: {ex.Message}";
                messageArea.Foreground = new SolidColorBrush(Colors.Red);
            }
        }

        private void FormatJson_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var parsedJson = Newtonsoft.Json.Linq.JToken.Parse(jsonEditor.Text);
                jsonEditor.Text = parsedJson.ToString(Newtonsoft.Json.Formatting.Indented);
                messageArea.Text = "JSON formatted successfully.";
                messageArea.Foreground = new SolidColorBrush(Colors.Green);
            }
            catch (Newtonsoft.Json.JsonReaderException ex)
            {
                messageArea.Text = $"Error formatting JSON: {ex.Message}";
                messageArea.Foreground = new SolidColorBrush(Colors.Red);
            }
        }


        private void BackgroundSelectionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (backgroundSelectionComboBox.SelectedItem is ComboBoxItem selectedColor)
            {
                string colorCode = selectedColor.Tag.ToString();
                var backgroundColor = (Color)ColorConverter.ConvertFromString(colorCode);
                jsonEditor.Background = new SolidColorBrush(backgroundColor);

                if (colorCode.Equals("#1E1E1E", StringComparison.OrdinalIgnoreCase))
                {
                    jsonEditor.Foreground = new SolidColorBrush(Colors.White);
                }
                else
                {
                    jsonEditor.Foreground = new SolidColorBrush(Colors.Black);
                }
            }
        }


        // End Region Json Editor

        // Begin Region Json Query

        private void ConfigureAvalonEdit()
        {
            jsonEditor.ShowLineNumbers = true;
            jsonEditor.TextArea.TextEntering += TextArea_TextEntering;
            jsonEditor.TextArea.TextEntered += TextArea_TextEntered;

            LoadJsonSyntaxHighlighting();
        }

        private void LoadJsonSyntaxHighlighting()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "ParseidonJson.JsonSyntaxHighlighting.xshd";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                {
                    using (XmlReader reader = XmlReader.Create(stream))
                    {
                        var highlighting = ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader.Load(reader,
                            ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance);

                        jsonEditor.SyntaxHighlighting = highlighting;
                    }
                }
                else
                {
                    MessageBox.Show("Unable to find the JSON syntax highlighting definition file.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExecuteQuery_Click(object sender, RoutedEventArgs e)
        {
            string jsonText = jsonDirectInput.Text;

            if (string.IsNullOrWhiteSpace(jsonText) && !string.IsNullOrWhiteSpace(filePathInput.Text))
            {
                try
                {
                    jsonText = File.ReadAllText(filePathInput.Text);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to load JSON from file: {ex.Message}", "Error", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }
            }

            string queryString = queryInput.Text;

            if (string.IsNullOrEmpty(jsonText) || string.IsNullOrEmpty(queryString))
            {
                MessageBox.Show("Please provide both JSON data and a query.", "Missing Data", MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            try
            {
                _jsonQuery.LoadJson(jsonText);
                var results = _jsonQuery.QueryJson(queryString);

                if (results is Newtonsoft.Json.Linq.JArray || results is Newtonsoft.Json.Linq.JObject)
                {
                    queryResults.Text = results.ToString(Newtonsoft.Json.Formatting.Indented);
                }
                else
                {
                    queryResults.Text = "No results found.";
                }
            }
            catch (Exception ex)
            {
                queryResults.Text = $"Error executing query: {ex.Message}";
            }
        }


        private void SelectJsonFile_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "JSON Files (*.json)|*.json|All files (*.*)|*.*",
                Title = "Select a JSON File"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                if (Path.GetExtension(filePath).Equals(".json", StringComparison.OrdinalIgnoreCase))
                {
                    filePathInput.Text = filePath;
                }
                else
                {
                    MessageBox.Show("Please select a valid JSON file.", "File Type Error", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        private void ClearAll_Click(object sender, RoutedEventArgs e)
        {
            filePathInput.Clear();
            queryInput.Clear();
            jsonDirectInput.Clear();
            queryResults.Clear();
            ResetJsonDirectInput();
        }

        private void ResetJsonDirectInput()
        {
            jsonDirectInput.Text = Placeholder;
            jsonDirectInput.Foreground = Brushes.Gray;

            if (!jsonDirectInput.IsFocused)
            {
                jsonDirectInput_GotFocus(jsonDirectInput, null);
            }
        }

        private void jsonDirectInput_GotFocus(object sender, RoutedEventArgs e)
        {
            if (jsonDirectInput.Text == Placeholder)
            {
                jsonDirectInput.Text = "";
                jsonDirectInput.Foreground = Brushes.Black;
            }
        }

        private void jsonDirectInput_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(jsonDirectInput.Text))
            {
                ResetJsonDirectInput();
            }
        }

        private void TextArea_TextEntered(object sender, TextCompositionEventArgs e)
        {
            if (e.Text == "{")
            {
                jsonEditor.Document.Insert(jsonEditor.CaretOffset, "}");
                jsonEditor.CaretOffset--;
            }
        }

        private void TextArea_TextEntering(object sender, TextCompositionEventArgs e)
        {
            if (e.Text == "\"")
            {
                var offset = jsonEditor.CaretOffset;
                jsonEditor.Document.Insert(offset, "\"\"");
                jsonEditor.CaretOffset = offset + 1;
                e.Handled = true;
            }
            else if (e.Text == "[")
            {
                var offset = jsonEditor.CaretOffset;
                jsonEditor.Document.Insert(offset, "[]");
                jsonEditor.CaretOffset = offset + 1;
                e.Handled = true;
            }
            else if (e.Text == "{")
            {
                var offset = jsonEditor.CaretOffset;
                jsonEditor.Document.Insert(offset, "{}");
                jsonEditor.CaretOffset = offset + 1;
                e.Handled = true;
            }
        }

        // End Region Json Query
    }
}