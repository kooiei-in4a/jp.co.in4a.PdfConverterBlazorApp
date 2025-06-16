using PdfConverterShare.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace PdfConverterApp.Services
{
    public class PdfConverterService : IPdfConverterService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;
        private readonly JsonSerializerOptions _jsonOptions;

        public PdfConverterService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            //https://localhost:7215
            _apiBaseUrl = "https://localhost:7215/api/dataconvert";

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };

            // タイムアウト設定
            _httpClient.Timeout = TimeSpan.FromMinutes(5);
        }

        public async Task<ConvertResponse?> ConvertPdfAsync(ConvertRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(_apiBaseUrl, request, _jsonOptions);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ConvertResponse>(_jsonOptions);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ErrorResponse? errorResponse = null;

                    try
                    {
                        errorResponse = JsonSerializer.Deserialize<ErrorResponse>(errorContent, _jsonOptions);
                    }
                    catch
                    {
                        // JSON解析失敗時は生のエラー内容を使用
                    }

                    var errorMessage = errorResponse?.Message ?? $"APIエラー (ステータス: {response.StatusCode})";
                    var details = errorResponse?.Details ?? errorContent;

                    throw new HttpRequestException($"{errorMessage}。詳細: {details}");
                }
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                throw new TimeoutException("API呼び出しがタイムアウトしました。", ex);
            }
            catch (HttpRequestException)
            {
                throw; // 再スロー
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"API通信中に予期しないエラーが発生しました: {ex.Message}", ex);
            }
        }

        public async Task<bool> CheckApiHealthAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync(_apiBaseUrl);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}