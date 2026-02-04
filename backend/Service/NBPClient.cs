using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using api.Interfaces;
using api.Models;

namespace api.Services
{
    public class NBPClient : INBPClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "https://api.nbp.pl/api/";

        public NBPClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(_baseUrl);
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        public async Task<List<ResponseItem>> GetExchangeRatesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("exchangerates/tables/c");
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                
                return JsonSerializer.Deserialize<List<ResponseItem>>(content, options) ?? new List<ResponseItem>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to fetch exchange rates: {ex.Message}", ex);
            }
        }
    }
} 