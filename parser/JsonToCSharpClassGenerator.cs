using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace ParseidonJson.parser
{
    public class JsonToCSharpClassGenerator : IJsonToCSharpClassGenerator
    {
        public double LastOperationElapsedTimeMs { get; private set; }

        public string GenerateCSharpClasses(string json)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            var classes = new StringBuilder();
            ParseJson(json, classes);
            stopwatch.Stop();
            LastOperationElapsedTimeMs = stopwatch.Elapsed.TotalMilliseconds;
            return classes.ToString();
        }

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
                    // Handling for primitive types or unsupported JSON structures at the root
                    classes.AppendLine("// The JSON does not contain a root object or array.");
                    break;
            }
        }

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

        private string DetermineCSharpType(JsonValueKind kind)
        {
            return kind switch
            {
                JsonValueKind.String => "string",
                JsonValueKind.Number => "double", // Using double to accommodate both float and integer
                JsonValueKind.True => "bool",
                JsonValueKind.False => "bool",
                _ => "object",
            };
        }

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
                // Update RootArray class definition to accommodate non-object array items directly
                classes.AppendLine("// Root array contains non-object elements. Adjust the RootArray class accordingly.");
            }
        }

        private string ConvertToPascalCase(string name)
        {
            return char.ToUpper(name[0]) + name.Substring(1);
        }
    }
}
