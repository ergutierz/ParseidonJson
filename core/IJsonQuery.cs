using Newtonsoft.Json.Linq;

namespace ParseidonJson.core
{
    /// <summary>
    /// Provides functionality to load JSON data and perform queries on it.
    /// </summary>
    public interface IJsonQuery
    {
        /// <summary>
        /// Loads JSON data into the parser.
        /// </summary>
        /// <param name="json">The JSON string to be loaded.</param>
        /// <remarks>
        /// This method must be called before performing any queries on the JSON data. 
        /// If the JSON string is not valid, an exception specific to JSON parsing may be thrown.
        /// </remarks>
        void LoadJson(string json);
        
        /// <summary>
        /// Performs a query on the loaded JSON data and returns the result.
        /// </summary>
        /// <param name="query">The query string, following a syntax that is understood by the implementation.</param>
        /// <returns>A <see cref="JToken"/> that represents the result of the query. The specific type of <see cref="JToken"/> returned can vary depending on the query.</returns>
        /// <exception cref="InvalidOperationException">Thrown if <see cref="LoadJson"/> has not been called prior to this method.</exception>
        /// <exception cref="JsonException">Thrown if the query is malformed or cannot be executed on the loaded JSON.</exception>
        JToken QueryJson(string query);
    }
}