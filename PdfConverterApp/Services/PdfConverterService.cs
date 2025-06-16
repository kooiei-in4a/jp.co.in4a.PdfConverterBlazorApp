using PdfConverterShare.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace PdfConverterApp.Services
{
    public class PdfConverterService : IPdfConverterService
    {
        private readonly HttpClient _httpClient;
        private readonly ApiSettings _apiSettings;
        private readonly string _apiBaseUrl;

        public PdfConverterService(HttpClient httpClient, ApiSettings apiSettings)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _apiSettings = apiSettings ?? throw new ArgumentNullException(nameof(apiSettings));
            _apiBaseUrl = _apiSettings.BaseUrl;

            // タイムアウト設定
            _httpClient.Timeout = TimeSpan.FromSeconds(_apiSettings.TimeoutSeconds);
        }

        public async Task<ConvertResponse?> ConvertPdfAsync(ConvertRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(_apiBaseUrl, request);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ConvertResponse>();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"API呼び出しエラー: {response.StatusCode}, {errorContent}");
                }
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                throw new TimeoutException($"API呼び出しがタイムアウトしました。({_apiSettings.TimeoutSeconds}秒)", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"PDF変換処理でエラーが発生しました: {ex.Message}", ex);
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

        /// <summary>
        /// 現在のAPI設定を取得
        /// </summary>
        /// <returns>API設定</returns>
        public ApiSettings GetApiSettings()
        {
            return _apiSettings;
        }
    }
}