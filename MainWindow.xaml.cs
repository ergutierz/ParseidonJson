using System.Windows;
// Assuming JsonToCSharpClassGenerator is correctly implemented and available in the namespace
using ParseidonJson.parser;

namespace ParseidonJson
{
    public partial class MainWindow : Window
    {
        private readonly IJsonParser _jsonParser;

        // Assuming you have a default constructor for scenarios without dependency injection
        public MainWindow()
        {
            InitializeComponent();
            // Initialize components or manually fetch dependencies if necessary
        }

        // Constructor with dependency injection for the IJsonParser
        public MainWindow(IJsonParser jsonParser) : this()
        {
            _jsonParser = jsonParser;
        }

        private void submitButton_Click(object sender, RoutedEventArgs e)
        {
            // Use the input from jsonInputBox to generate C# class definitions
            string jsonContent = jsonInputBox.Text;
            var generator = new JsonToCSharpClassGenerator();
            var csharpCode = generator.GenerateCSharpClasses(jsonContent);

            // Display the generated C# class definitions in the csharpOutputBox
            csharpOutputBox.Text = csharpCode;
        }

        private void clearButton_Click(object sender, RoutedEventArgs e)
        {
            // Clear both the input and output TextBoxes
            jsonInputBox.Text = string.Empty;
            csharpOutputBox.Text = string.Empty;
        }
    }
}