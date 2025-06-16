using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PdfConverterApp;
using PdfConverterApp.Services;


namespace PdfConverterApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            // HttpClient設定
            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            // サービス登録
            builder.Services.AddScoped<IPdfConverterService, PdfConverterService>();

            // ログ設定
            builder.Logging.SetMinimumLevel(LogLevel.Information);

            await builder.Build().RunAsync();
        }
    }
}
