using System.Text.Json;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using PdfConverterApi.Models; // CorsSettings, RateLimitingSettings�̖��O���

namespace PdfConverterApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // --- �ݒ�N���X�̃C���X�^���X���ƃo�C���h ---
            var corsSettings = new CorsSettings();
            builder.Configuration.GetSection("CorsSettings").Bind(corsSettings);

            var rateLimitSettings = new RateLimitingSettings();
            builder.Configuration.GetSection("RateLimitingSettings").Bind(rateLimitSettings);


            // --- �T�[�r�X�ݒ� ---
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                    options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
                });

            // �����t��CORS�ݒ�
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

            // �����t�����[�g�����ݒ� (appsettings.json����ǂݍ���)
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

            // --- HTTP���N�G�X�g�p�C�v���C���̐ݒ� ---
            // �J�����p��Swagger UI�ݒ�͍폜�ς�

            app.UseHttpsRedirection();

            // �����t��CORS�~�h���E�F�A�̗L����
            if (!string.IsNullOrEmpty(corsSettings.PolicyName) && corsSettings.AllowedOrigins?.Length > 0)
            {
                app.UseCors(corsSettings.PolicyName);
            }

            // �����t�����[�g�����~�h���E�F�A�̗L����
            if (rateLimitSettings.EnableRateLimiting)
            {
                app.UseRateLimiter();
            }

            app.UseAuthorization();
            app.MapControllers();

            app.Logger.LogInformation("PDF�ϊ�API �T�[�o�[�N������: {Environment}", app.Environment.EnvironmentName);
            app.Run();
        }
    }
}