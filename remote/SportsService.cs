namespace ParseidonJson.remote;

using System;
using System.Net.Http;
using System.Threading.Tasks;

public class SportsService : IDisposable
    {
        private readonly HttpClient _httpClient;

        public SportsService()
        {
            _httpClient = new HttpClient();
        }

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

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
