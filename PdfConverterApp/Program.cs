using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PdfConverterApp;
using PdfConverterApp.Services;
using PdfConverterShare.Models;


namespace PdfConverterApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            // API設定の読み込みと登録
            var apiSettings = new ApiSettings();
            builder.Configuration.GetSection("ApiSettings").Bind(apiSettings);
            // 設定の妥当性チェック
            if (!apiSettings.IsValid())
            {
                throw new InvalidOperationException("API設定が無効です。appsettings.jsonを確認してください。");
            }
            // ApiSettingsをDIコンテナに登録
            builder.Services.AddSingleton(apiSettings);

            // HttpClient設定
            //builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            // ▼▼▼ HttpClient設定をここから変更 ▼▼▼
            builder.Services.AddScoped(sp =>
            {
                // DIコンテナからApiSettingsを取得
                var settings = sp.GetRequiredService<ApiSettings>();

                // ApiSettingsのBaseUrlをHttpClientのBaseAddressに設定
                return new HttpClient { BaseAddress = new Uri(settings.BaseUrl!) };
            });
            // ▲▲▲ HttpClient設定をここまで変更 ▲▲▲

            // サービス登録
            builder.Services.AddScoped<IPdfConverterService, PdfConverterService>();

            // ログ設定
            builder.Logging.SetMinimumLevel(LogLevel.Information);

            await builder.Build().RunAsync();
        }
    }
}
