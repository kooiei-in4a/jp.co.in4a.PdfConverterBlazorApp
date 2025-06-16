using System.Text.Json;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using PdfConverterApi.Models; // CorsSettings, RateLimitingSettingsの名前空間

namespace PdfConverterApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // --- 設定クラスのインスタンス化とバインド ---
            var corsSettings = new CorsSettings();
            builder.Configuration.GetSection("CorsSettings").Bind(corsSettings);

            var rateLimitSettings = new RateLimitingSettings();
            builder.Configuration.GetSection("RateLimitingSettings").Bind(rateLimitSettings);


            // --- サービス設定 ---
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                    options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
                });

            // 条件付きCORS設定
            if (!string.IsNullOrEmpty(corsSettings.PolicyName) && corsSettings.AllowedOrigins?.Length > 0)
            {
                builder.Services.AddCors(options =>
                {
                    options.AddPolicy(corsSettings.PolicyName, policy =>
                    {
                        policy.WithOrigins(corsSettings.AllowedOrigins)
                              .WithExposedHeaders(corsSettings.ExposedHeaders);

                        if (corsSettings.AllowAnyHeader) policy.AllowAnyHeader();
                        if (corsSettings.AllowAnyMethod) policy.AllowAnyMethod();
                    });
                });
            }

            // 条件付きレート制限設定 (appsettings.jsonから読み込み)
            if (rateLimitSettings.EnableRateLimiting)
            {
                builder.Services.AddRateLimiter(options =>
                {
                    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                    var policies = rateLimitSettings.Policies;

                    options.AddFixedWindowLimiter("minute", opt =>
                    {
                        opt.PermitLimit = policies.Minute.PermitLimit;
                        opt.Window = TimeSpan.FromSeconds(policies.Minute.WindowInSeconds);
                        opt.QueueLimit = 0;
                    });
                });
            }

            var app = builder.Build();

            // --- HTTPリクエストパイプラインの設定 ---
            // 開発環境用のSwagger UI設定は削除済み

            app.UseHttpsRedirection();

            // 条件付きCORSミドルウェアの有効化
            if (!string.IsNullOrEmpty(corsSettings.PolicyName) && corsSettings.AllowedOrigins?.Length > 0)
            {
                app.UseCors(corsSettings.PolicyName);
            }

            // 条件付きレート制限ミドルウェアの有効化
            if (rateLimitSettings.EnableRateLimiting)
            {
                app.UseRateLimiter();
            }

            app.UseAuthorization();
            app.MapControllers();

            app.Logger.LogInformation("PDF変換API サーバー起動完了: {Environment}", app.Environment.EnvironmentName);
            app.Run();
        }
    }
}