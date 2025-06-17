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

            // API�ݒ�̓ǂݍ��݂Ɠo�^
            var apiSettings = new ApiSettings();
            builder.Configuration.GetSection("ApiSettings").Bind(apiSettings);
            // �ݒ�̑Ó����`�F�b�N
            if (!apiSettings.IsValid())
            {
                throw new InvalidOperationException("API�ݒ肪�����ł��Bappsettings.json���m�F���Ă��������B");
            }
            // ApiSettings��DI�R���e�i�ɓo�^
            builder.Services.AddSingleton(apiSettings);

            // HttpClient�ݒ�
            //builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            // ������ HttpClient�ݒ����������ύX ������
            builder.Services.AddScoped(sp =>
            {
                // DI�R���e�i����ApiSettings���擾
                var settings = sp.GetRequiredService<ApiSettings>();

                // ApiSettings��BaseUrl��HttpClient��BaseAddress�ɐݒ�
                return new HttpClient { BaseAddress = new Uri(settings.BaseUrl!) };
            });
            // ������ HttpClient�ݒ�������܂ŕύX ������

            // �T�[�r�X�o�^
            builder.Services.AddScoped<IPdfConverterService, PdfConverterService>();

            // ���O�ݒ�
            builder.Logging.SetMinimumLevel(LogLevel.Information);

            await builder.Build().RunAsync();
        }
    }
}
