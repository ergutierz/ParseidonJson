namespace ParseidonJson.core
{
    /// <summary>
    /// Defines a contract for a JSON parser that can generate C# class definitions from a JSON string.
    /// </summary>
    public interface IJsonParser
    {
        /// <summary>
        /// Generates C# classes that represent the structure of the provided JSON string.
        /// </summary>
        /// <param name="json">The JSON string to parse and generate C# class definitions for.</param>
        /// <returns>A string containing the C# class definitions that represent the JSON structure.</returns>
        /// <exception cref="ArgumentException">Thrown when the provided JSON string is not valid.</exception>
        string GenerateCSharpClasses(string json);
        
        /// <summary>
        /// Gets the elapsed time in milliseconds of the last operation performed by the parser.
        /// </summary>
        /// <value>The time in milliseconds it took to complete the last parsing operation.</value>
        double LastOperationElapsedTimeMs { get; }
    }
}