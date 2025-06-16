using System.Text.Json;

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

            // CORS設定
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowBlazorWasm", policy =>
                {
                    policy.WithOrigins("https://localhost:7072", "http://localhost:5122")
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .WithExposedHeaders("Content-Disposition");
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
            app.UseCors("AllowBlazorWasm");
            app.UseAuthorization();
            app.MapControllers();

            // 起動ログ
            app.Logger.LogInformation("PDF変換API サーバー起動完了: {Environment}", app.Environment.EnvironmentName);

            app.Run();
        }
    }
}
