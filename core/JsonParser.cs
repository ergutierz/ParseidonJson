using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace ParseidonJson.core
{
    /// <summary>
    /// A JSON parser that generates C# class definitions from a given JSON string.
    /// </summary>
    public class JsonParser : IJsonParser
    {
        /// <summary>
        /// Gets the elapsed time in milliseconds of the last JSON parsing operation.
        /// </summary>
        public double LastOperationElapsedTimeMs { get; private set; }

        /// <summary>
        /// Parses a JSON string and generates C# class definitions that represent the JSON structure.
        /// </summary>
        /// <param name="json">The JSON string to parse.</param>
        /// <returns>A string containing the generated C# class definitions.</returns>
        public string GenerateCSharpClasses(string json)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            var classes = new StringBuilder();
            ParseJson(json, classes);
            stopwatch.Stop();
            LastOperationElapsedTimeMs = stopwatch.Elapsed.TotalMilliseconds;
            return classes.ToString();
        }

        /// <summary>
        /// Parses the JSON string and appends generated class definitions to the provided StringBuilder.
        /// </summary>
        /// <param name="json">The JSON string to parse.</param>
        /// <param name="classes">The StringBuilder to which class definitions are appended.</param>
        private void ParseJson(string json, StringBuilder classes)
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            switch (root.ValueKind)
            {
                case JsonValueKind.Object:
                    GenerateClassFromElement("RootObject", root, classes, isRoot: true);
                    break;
                case JsonValueKind.Array:
                    HandleRootLevelArray(root, classes);
                    break;
                default:
                    classes.AppendLine("// The JSON does not contain a root object or array.");
                    break;
            }
        }

        /// <summary>
        /// Generates C# class definitions from a JsonElement and appends them to the provided StringBuilder.
        /// </summary>
        /// <param name="className">The name to use for the generated class.</param>
        /// <param name="element">The JsonElement to generate the class from.</param>
        /// <param name="classes">The StringBuilder to which the class definition is appended.</param>
        /// <param name="isRoot">Indicates whether the element is the root object.</param>
        private void GenerateClassFromElement(string className, JsonElement element, StringBuilder classes, bool isRoot = false)
        {
            if (!isRoot && !className.EndsWith("Class"))
            {
                className += "Class";
            }

            classes.AppendLine($"public class {className}");
            classes.AppendLine("{");

            foreach (var property in element.EnumerateObject())
            {
                string propName = ConvertToPascalCase(property.Name);
                string propType = DeterminePropertyType(property.Value, propName, classes);
                classes.AppendLine($"    public {propType} {propName} {{ get; set; }}");
            }

            classes.AppendLine("}");
            classes.AppendLine();
        }

        /// <summary>
        /// Determines the C# type that corresponds to the given JsonElement and appends any necessary class definitions to the provided StringBuilder.
        /// </summary>
        /// <param name="element">The JsonElement to determine the type for.</param>
        /// <param name="propName">The name of the property that this element represents.</param>
        /// <param name="classes">The StringBuilder to which any generated class definitions are appended.</param>
        /// <returns>The C# type as a string that represents the JsonElement's type.</returns>
        private string DeterminePropertyType(JsonElement element, string propName, StringBuilder classes)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    var nestedClassName = $"{propName}Class";
                    GenerateClassFromElement(nestedClassName, element, classes);
                    return nestedClassName;
                case JsonValueKind.Array:
                    return $"List<{DetermineArrayType(element, propName, classes)}>";
                default:
                    return DetermineCSharpType(element.ValueKind);
            }
        }

        /// <summary>
        /// Determines the C# type for elements within an array and generates nested class definitions if necessary.
        /// </summary>
        /// <param name="arrayElement">The JsonElement representing the array to analyze.</param>
        /// <param name="propName">The proposed name for the property, used to generate class names for object elements within the array.</param>
        /// <param name="classes">The StringBuilder instance to which generated class definitions are appended.</param>
        /// <returns>A string representing the C# type of the array's elements. This will be a class name for object elements, or a primitive type for others.</returns>
        private string DetermineArrayType(JsonElement arrayElement, string propName, StringBuilder classes)
        {
            var firstElement = arrayElement.EnumerateArray().FirstOrDefault();
            if (firstElement.ValueKind == JsonValueKind.Object)
            {
                var itemClassName = $"{propName}ItemClass";
                GenerateClassFromElement(itemClassName, firstElement, classes);
                return itemClassName;
            }
            else
            {
                return DetermineCSharpType(firstElement.ValueKind);
            }
        }

        /// <summary>
        /// Converts a JsonValueKind to its corresponding C# type as a string.
        /// </summary>
        /// <param name="kind">The JsonValueKind to convert.</param>
        /// <returns>A string representing the C# type that corresponds to the specified JsonValueKind.</returns>
        /// <remarks>
        /// This method maps JSON types to their closest C# primitive types or to 'object' if no direct mapping exists.
        /// </remarks>
        private string DetermineCSharpType(JsonValueKind kind)
        {
            return kind switch
            {
                JsonValueKind.String => "string",
                JsonValueKind.Number => "double", 
                JsonValueKind.True => "bool",
                JsonValueKind.False => "bool",
                _ => "object",
            };
        }

        /// <summary>
        /// Handles parsing of root level arrays by generating a wrapper class with a list of items.
        /// </summary>
        /// <param name="root">The root JsonElement representing an array.</param>
        /// <param name="classes">The StringBuilder to which the class definition is appended.</param>
        private void HandleRootLevelArray(JsonElement root, StringBuilder classes)
        {
            classes.AppendLine("public class RootArray");
            classes.AppendLine("{");
            classes.AppendLine("    public List<RootItem> Items { get; set; } = new List<RootItem>();");
            classes.AppendLine("}");

            var firstElement = root.EnumerateArray().FirstOrDefault();
            if (firstElement.ValueKind == JsonValueKind.Object)
            {
                GenerateClassFromElement("RootItem", firstElement, classes);
            }
            else
            {
                classes.AppendLine("// Root array contains non-object elements. Adjust the RootArray class accordingly.");
            }
        }

        /// <summary>
        /// Converts a string to PascalCase.
        /// </summary>
        /// <param name="name">The string to convert.</param>
        /// <returns>The converted PascalCase string.</returns>
        private string ConvertToPascalCase(string name)
        {
            return char.ToUpper(name[0]) + name.Substring(1);
        }
    }
}
