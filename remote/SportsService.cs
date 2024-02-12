namespace ParseidonJson.remote;

using System;
using System.Net.Http;
using System.Threading.Tasks;

/// <summary>
/// Represents a service for fetching sports statistics from a remote source.
/// </summary>
public class SportsService : IDisposable
    {
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="SportsService"/> class.
        /// </summary>
        public SportsService()
        {
            _httpClient = new HttpClient();
        }

        /// <summary>
        /// Asynchronously fetches sports statistics as a JSON string from the specified URL.
        /// </summary>
        /// <param name="url">The URL from which to fetch sports statistics.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. 
        /// The task result contains the JSON string of sports statistics. 
        /// Returns <c>null</c> if an error occurs during the request.
        /// </returns>
        /// <exception cref="HttpRequestException">Thrown when the request fails due to an underlying issue such as network connectivity, DNS failure, server certificate validation, or timeout.</exception>
        public async Task<string> FetchSportsStatsAsync(string url)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string json = await response.Content.ReadAsStringAsync();
                return json;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"\nException Caught!");
                Console.WriteLine($"Message :{e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="SportsService"/> and optionally disposes of the managed resources.
        /// </summary>
        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
