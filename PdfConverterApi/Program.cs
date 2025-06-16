using System.Text.Json;

namespace PdfConverterApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // �T�[�r�X�ݒ�
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    // JSON�ݒ�̍œK��
                    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                    options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
                });

            // CORS�ݒ�
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

            // ���O�ݒ�
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

            // �N�����O
            app.Logger.LogInformation("PDF�ϊ�API �T�[�o�[�N������: {Environment}", app.Environment.EnvironmentName);

            app.Run();
        }
    }
}
