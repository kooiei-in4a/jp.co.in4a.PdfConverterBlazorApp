using System.Text.Json;
using PdfConverterApi.Models;

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

            // CORS�ݒ��appsettings.json����ǂݍ��݁i�^���S�ȕ��@�j
            var corsSettings = new CorsSettings();
            builder.Configuration.GetSection("CorsSettings").Bind(corsSettings);

            //// �o���f�[�V����
            //if (corsSettings.AllowedOrigins.Length == 0)
            //{
            //    // �f�t�H���g�l��ݒ�
            //    corsSettings.AllowedOrigins = new[] { "https://localhost:7072", "http://localhost:5122" };
            //    builder.Services.Configure<ILogger>(logger =>
            //    {
            //        // �x�����O�͋N����ɏo�͂��邽�߁A�����ł̓R�����g�A�E�g
            //        // logger.LogWarning("CorsSettings:AllowedOrigins ���ݒ肳��Ă��Ȃ����߁A�f�t�H���g�l���g�p���܂��B");
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
            app.UseCors(corsSettings.PolicyName);
            app.UseAuthorization();
            app.MapControllers();

            // �N�����O
            app.Logger.LogInformation("PDF�ϊ�API �T�[�o�[�N������: {Environment}", app.Environment.EnvironmentName);

            app.Run();
        }
    }
}