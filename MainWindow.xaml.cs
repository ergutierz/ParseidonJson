using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using ParseidonJson.editor;
using ParseidonJson.parser;
using ParseidonJson.remote;

namespace ParseidonJson
{
    public partial class MainWindow : Window
    {
        private readonly SportsService _sportsService;
        private readonly IJsonToCSharpClassGenerator _jsonToCSharpClassGenerator;
        private readonly IJsonEditor _jsonEditor;
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
            IJsonToCSharpClassGenerator jsonToCSharpClassGenerator,
            IJsonEditor jsonEditor
        ) : this()
        {
            _sportsService = sportsService;
            _jsonToCSharpClassGenerator = jsonToCSharpClassGenerator;
            _jsonEditor = jsonEditor;
        }

        // Begin Region Json Parsing

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

            string url =
                "http://sports.snoozle.net/search/nfl/searchHandler?fileType=inline&statType=teamStats&season=2020&teamName=26";
            string jsonContent = string.Empty;

            try
            {
                jsonContent = await Task.Run(() => _sportsService.FetchSportsStatsAsync(url).Result);

                var dataModel = await Task.Run(() => _jsonToCSharpClassGenerator.GenerateCSharpClasses(jsonContent));

                jsonInputBox.Text = jsonContent;
                outputBox.Text = dataModel;
                elapsedTimeLabel.Content =
                    $"Elapsed Time for Processing: {_jsonToCSharpClassGenerator.LastOperationElapsedTimeMs:F2}ms";
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
            // Clear the editor for a new JSON object
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
        
                // Ensure AvalonEdit.Text property is used for AvalonEdit instance
                File.WriteAllText(filename, jsonEditor.Text);
        
                messageArea.Text = "JSON saved successfully.";
                messageArea.Foreground = new SolidColorBrush(Colors.Green);
            }
        }

        private void ValidateJson_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Using Newtonsoft.Json for parsing which throws a JsonReaderException if JSON is invalid
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
                // Parsing the JSON text to a dynamic object and re-serializing it with indentation
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

                // Check if the selected background color is black
                if (colorCode.Equals("#1E1E1E", StringComparison.OrdinalIgnoreCase))
                {
                    // Change the text color to white
                    jsonEditor.Foreground = new SolidColorBrush(Colors.White);
                }
                else
                {
                    // Change the text color to black or any other default color
                    jsonEditor.Foreground = new SolidColorBrush(Colors.Black);
                }
            }
        }


        // End Region Json Editor

        // Begin Region Json Query

        private void ConfigureAvalonEdit()
        {
            // Example configuration for AvalonEdit
            jsonEditor.ShowLineNumbers = true;
            jsonEditor.TextArea.TextEntering += TextArea_TextEntering;
            jsonEditor.TextArea.TextEntered += TextArea_TextEntered;

            // Load syntax highlighting, assuming you have the JSON definition available
            LoadJsonSyntaxHighlighting();
        }

        private void LoadJsonSyntaxHighlighting()
        {
            // Path to the .xshd file
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "ParseidonJson.JsonSyntaxHighlighting.xshd";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                {
                    using (XmlReader reader = XmlReader.Create(stream))
                    {
                        // Load the .xshd file
                        var highlighting = ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader.Load(reader,
                            ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance);

                        // Apply the highlighting to the editor
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
            // Retrieve JSON data either from the direct input box or a file
            string
                jsonText = jsonDirectInput
                    .Text; // This assumes direct input; you might also load from filePathInput.Text if needed

            if (string.IsNullOrWhiteSpace(jsonText) && !string.IsNullOrWhiteSpace(filePathInput.Text))
            {
                // Load JSON from file if direct input is empty but a file path is provided
                try
                {
                    jsonText = System.IO.File.ReadAllText(filePathInput.Text);
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

            // Initialize the JsonEditor with the provided JSON text
            var jsonEditor = new JsonEditor();
            try
            {
                jsonEditor.LoadJson(jsonText);
                var results = jsonEditor.QueryJson(queryString);

                // Handle both single and multiple results
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
                Filter = "JSON Files (*.json)|*.json|All files (*.*)|*.*", // Filter for JSON files
                Title = "Select a JSON File"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                // Validate the file type if necessary
                string filePath = openFileDialog.FileName;
                if (System.IO.Path.GetExtension(filePath).Equals(".json", StringComparison.OrdinalIgnoreCase))
                {
                    filePathInput.Text = filePath;
                    // Optional: Load and display the JSON file content in queryInput or process it directly
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
            // Clear all text inputs and outputs
            filePathInput.Clear();
            queryInput.Clear();
            jsonDirectInput.Clear();
            queryResults.Clear();

            // Optionally, reset the placeholder text for jsonDirectInput
            ResetJsonDirectInput();
        }

        private void ResetJsonDirectInput()
        {
            jsonDirectInput.Text =
                Placeholder; // Assuming Placeholder is a class-level constant for the placeholder text
            jsonDirectInput.Foreground = Brushes.Gray;
            // This checks if it's necessary to reset the placeholder. If the box is focused, it won't set the placeholder text.
            if (!jsonDirectInput.IsFocused)
            {
                jsonDirectInput_GotFocus(jsonDirectInput,
                    null); // You might not need this if you always want the placeholder text reset here
            }
        }

        private void jsonDirectInput_GotFocus(object sender, RoutedEventArgs e)
        {
            if (jsonDirectInput.Text == Placeholder)
            {
                jsonDirectInput.Text = "";
                jsonDirectInput.Foreground = Brushes.Black; // Change text color back to default
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
            // Your logic here, e.Text will contain the text that was entered
            if (e.Text == "{")
            {
                // For example, automatically insert a closing brace
                jsonEditor.Document.Insert(jsonEditor.CaretOffset, "}");
                // Adjust the caret position if necessary
                jsonEditor.CaretOffset--;
            }
        }

        private void TextArea_TextEntering(object sender, TextCompositionEventArgs e)
        {
            // Example logic for handling special character input
            if (e.Text == "\"") // Checks if the user is entering a double quote
            {
                var offset = jsonEditor.CaretOffset;
                jsonEditor.Document.Insert(offset, "\"\""); // Inserts a pair of double quotes
                jsonEditor.CaretOffset = offset + 1; // Moves the caret inside the quotes
                e.Handled = true; // Prevents the default handling (i.e., inserting a single quote)
            }
            else if (e.Text == "[")
            {
                var offset = jsonEditor.CaretOffset;
                jsonEditor.Document.Insert(offset, "[]"); // Inserts a pair of square brackets
                jsonEditor.CaretOffset = offset + 1; // Moves the caret inside the brackets
                e.Handled = true; // Prevents the default handling (i.e., inserting a single bracket)
            }
            else if (e.Text == "{")
            {
                var offset = jsonEditor.CaretOffset;
                jsonEditor.Document.Insert(offset, "{}"); // Inserts a pair of curly braces
                jsonEditor.CaretOffset = offset + 1; // Moves the caret inside the braces
                e.Handled = true; // Prevents the default handling (i.e., inserting a single brace)
            }
            // Add more conditions as needed for other special characters
        }


        // End Region Json Query
    }
}