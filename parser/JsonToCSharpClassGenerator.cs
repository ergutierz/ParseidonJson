namespace ParseidonJson.parser;

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

public class JsonToCSharpClassGenerator : IJsonToCSharpClassGenerator
{
    private HashSet<string> generatedClasses = new HashSet<string>();

    public string GenerateCSharpClasses(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        var classes = new StringBuilder();
        GenerateClassFromElement("Root", root, classes);
        return classes.ToString();
    }

    private void GenerateClassFromElement(string className, JsonElement element, StringBuilder classes)
    {
        if (generatedClasses.Contains(className))
            return;
        generatedClasses.Add(className);

        classes.AppendLine($"public class {className}");
        classes.AppendLine("{");

        foreach (var property in element.EnumerateObject())
        {
            string propName = ConvertToPascalCase(property.Name);
            string propType = DeterminePropertyType(property.Value, propName, classes);
            classes.AppendLine($"    public {propType} {propName} {{ get; set; }}");
        }

        classes.AppendLine("}\n");
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
                return DetermineArrayType(element, propName, classes);
            default:
                return DetermineCSharpType(element.ValueKind);
        }
    }

    private string DetermineArrayType(JsonElement arrayElement, string propName, StringBuilder classes)
    {
        if (!arrayElement.EnumerateArray().MoveNext()) // Check if the array is empty
        {
            return "List<object>"; // Default to List<object> for empty arrays
        }
        
        var firstElement = arrayElement.EnumerateArray().First();
        var elementType = DeterminePropertyType(firstElement, $"{propName}Item", classes);
        return $"List<{elementType}>";
    }

    private string DetermineCSharpType(JsonValueKind kind)
    {
        return kind switch
        {
            JsonValueKind.String => "string",
            JsonValueKind.Number => "int", // Simplify to int, though more nuance is needed for real cases
            JsonValueKind.True => "bool",
            JsonValueKind.False => "bool",
            // Removed JsonValueKind.Object and JsonValueKind.Array as those are handled in DeterminePropertyType
            _ => "object",
        };
    }

    private string ConvertToPascalCase(string name)
    {
        // Convert camelCase to PascalCase
        return char.ToUpper(name[0]) + name.Substring(1);
    }
}
