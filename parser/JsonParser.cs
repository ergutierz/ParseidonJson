using System.Dynamic;
using System.Text.RegularExpressions;

namespace ParseidonJson.parser;

public class JsonParser : IJsonParser
{
    public object Parse(string json)
    {
        // Remove spaces, newlines, tabs for simplicity (not efficient for large strings)
        string compactJson = Regex.Replace(json, @"\s", "");

        // Assume the JSON is an object: { "key": "value", ... }
        if (compactJson.StartsWith("{") && compactJson.EndsWith("}"))
        {
            dynamic result = new ExpandoObject();
            var dict = result as IDictionary<string, object>;

            // Remove the enclosing braces and split by commas not enclosed in quotes (very basic and error-prone)
            var pairs = Regex.Split(compactJson.Substring(1, compactJson.Length - 2), "(?<!\":.*),(?!.*:\")");

            foreach (var pair in pairs)
            {
                // Split by the first colon found (key:value)
                var parts = pair.Split(new[] { ':' }, 2);
                if (parts.Length == 2)
                {
                    // Remove the quotes from key and value (very basic)
                    var key = parts[0].Trim('"');
                    var value = parts[1].Trim('"');

                    dict[key] = value; // Add to the dictionary
                }
            }

            return result;
        }
        else
        {
            throw new ArgumentException("The provided JSON string is not a valid JSON object.");
        }
    }
}