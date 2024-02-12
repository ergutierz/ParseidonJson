using Newtonsoft.Json.Linq;

namespace ParseidonJson.core;

/// <summary>
/// Provides functionality to load JSON data and execute queries against it using JSONPath syntax.
/// </summary>
public class JsonQuery : IJsonQuery
{
    private JToken _jsonToken;

    /// <summary>
    /// Loads JSON data into the instance, preparing it for querying.
    /// </summary>
    /// <param name="json">The JSON string to be loaded.</param>
    /// <exception cref="InvalidOperationException">Thrown when the JSON string is malformed or cannot be parsed.</exception>
    public void LoadJson(string json)
    {
        try
        {
            _jsonToken = JToken.Parse(json);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to load JSON.", ex);
        }
    }
    
    /// <summary>
    /// Executes a query on the loaded JSON data using JSONPath syntax and returns the matching token(s).
    /// </summary>
    /// <param name="query">The JSONPath query string to execute against the loaded JSON data.</param>
    /// <returns>A <see cref="JToken"/> representing the result of the query. 
    /// If multiple tokens match the query, a <see cref="JArray"/> containing all matching tokens is returned. 
    /// Returns <c>null</c> if no matches are found.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the query is malformed or if querying fails for another reason.</exception>
    /// <remarks>
    /// The method requires that <see cref="LoadJson(string)"/> has been successfully called beforehand to load JSON data.
    /// If <see cref="LoadJson(string)"/> has not been called, this method will throw an InvalidOperationException.
    /// </remarks>
    public JToken QueryJson(string query)
    {
        try
        {
            var tokens = _jsonToken.SelectTokens(query);
            if (tokens.Any())
            {
                return tokens.Count() == 1 ? tokens.First() : new JArray(tokens);
            }

            return null;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Query failed: " + ex.Message, ex);
        }
    }
}