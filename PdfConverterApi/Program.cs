using System.Text.Json;
using PdfConverterApi.Models;

namespace PdfConverterApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // サービス設定
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    // JSON設定の最適化
                    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                    options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
                });

            // CORS設定をappsettings.jsonから読み込み（型安全な方法）
            var corsSettings = new CorsSettings();
            builder.Configuration.GetSection("CorsSettings").Bind(corsSettings);

            //// バリデーション
            //if (corsSettings.AllowedOrigins.Length == 0)
            //{
            //    // デフォルト値を設定
            //    corsSettings.AllowedOrigins = new[] { "https://localhost:7072", "http://localhost:5122" };
            //    builder.Services.Configure<ILogger>(logger =>
            //    {
            //        // 警告ログは起動後に出力するため、ここではコメントアウト
            //        // logger.LogWarning("CorsSettings:AllowedOrigins が設定されていないため、デフォルト値を使用します。");
            //    });
            //}

            builder.Services.AddCors(options =>
            {
                options.AddPolicy(corsSettings.PolicyName, policy =>
                {
                    policy.WithOrigins(corsSettings.AllowedOrigins);

                    if (corsSettings.AllowAnyHeader)
                    {
                        policy.AllowAnyHeader();
                    }

                    if (corsSettings.AllowAnyMethod)
                    {
                        policy.AllowAnyMethod();
                    }

                    if (corsSettings.ExposedHeaders.Length > 0)
                    {
                        policy.WithExposedHeaders(corsSettings.ExposedHeaders);
                    }
                });
            });

            // ログ設定
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Logging.AddDebug();

            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();
            app.UseCors(corsSettings.PolicyName);
            app.UseAuthorization();
            app.MapControllers();

            // 起動ログ
            app.Logger.LogInformation("PDF変換API サーバー起動完了: {Environment}", app.Environment.EnvironmentName);

            app.Run();
        }
    }
}