using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Aggregator
{
    public class FinnhubService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public FinnhubService(HttpClient httpClient)
        {
            _apiKey = Environment.GetEnvironmentVariable("FINNHUB_API_KEY")!;
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://finnhub.io/api/v1/");
        }

        public async Task<StockPriceModel> GetStockPriceAsync(string symbol)
        {
            var response = await _httpClient.GetFromJsonAsync<FinnhubResponse>($"quote?symbol={symbol}&token={_apiKey}");
            return new StockPriceModel
            {
                Symbol = symbol,
                CurrentPrice = response.CurrentPrice,
                HighPrice = response.HighPrice,
                LowPrice = response.LowPrice,
                OpenPrice = response.OpenPrice,
                PreviousClose = response.PreviousClose,
                Change = response.Change,
                PercentChange = response.PercentChange,
            };
        }
    }

    public class FinnhubResponse
    {
        [JsonPropertyName("c")]
        public decimal CurrentPrice { get; set; }
        
        [JsonPropertyName("h")]
        public decimal HighPrice { get; set; }
        
        [JsonPropertyName("l")]
        public decimal LowPrice { get; set; }
        
        [JsonPropertyName("o")]
        public decimal OpenPrice { get; set; }
        
        [JsonPropertyName("pc")]
        public decimal PreviousClose { get; set; }
        
        [JsonPropertyName("d")]
        public decimal Change { get; set; }
        
        [JsonPropertyName("dp")]
        public decimal PercentChange { get; set; }
    }
}
