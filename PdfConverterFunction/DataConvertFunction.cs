using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using System.ComponentModel.DataAnnotations;
using PdfConverterShare.Models;
using PdfSharpCore.Pdf.IO;
using jp.co.in4a.PdfSharpCoreWrapper.PasswordProtect.PdfSharpCore;

namespace PdfConverterFunction
{
    public class DataConvertFunction
    {
        private readonly ILogger<DataConvertFunction> _logger;
        private const int MAX_FILE_SIZE = 20 * 1024 * 1024; // 20MB

        public DataConvertFunction(ILogger<DataConvertFunction> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// API����m�F
        /// </summary>
        [Function("GetApiInfo")]
        public async Task<HttpResponseData> GetApiInfo(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "api/dataconvert")] HttpRequestData req)
        {
            _logger.LogInformation("API����m�F���N�G�X�g");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json");

            var apiInfo = new ApiInfoResponse();
            await response.WriteStringAsync(JsonSerializer.Serialize(apiInfo, GetJsonSerializerOptions()));

            return response;
        }

        /// <summary>
        /// PDF�t�@�C���ϊ�����
        /// </summary>
        [Function("ConvertPdf")]
        public async Task<HttpResponseData> ConvertPdf(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "api/dataconvert")] HttpRequestData req)
        {
            var requestId = Guid.NewGuid().ToString("N")[..8];

            try
            {
                // ���N�G�X�g�T�C�Y���O
                _logger.LogInformation("[{RequestId}] PDF�ϊ����N�G�X�g�J�n: Size={FileSize}bytes",requestId, req.Body.Length);


                // ���N�G�X�g�T�C�Y�`�F�b�N�iRequestSizeLimit�̑�ցj
                if (req.Body.Length > MAX_FILE_SIZE)
                {
                    _logger.LogWarning("[{RequestId}] �t�@�C���T�C�Y��������: {Size}bytes", requestId, req.Body.Length);
                    return await CreateErrorResponse(req, HttpStatusCode.BadRequest, new ErrorResponse
                    {
                        Message = "�t�@�C���T�C�Y�������𒴂��Ă��܂�",
                        Details = $"�ő�T�C�Y: {MAX_FILE_SIZE / (1024 * 1024)}MB"
                    });
                }

                // ���N�G�X�g�{�f�B�̓ǂݎ��i[FromBody]�̑�ցj
                string requestBody = await req.ReadAsStringAsync() ?? "";

                if (string.IsNullOrEmpty(requestBody))
                {
                    _logger.LogWarning("[{RequestId}] ���N�G�X�g�{�f�B����ł�", requestId);
                    return await CreateErrorResponse(req, HttpStatusCode.BadRequest, new ErrorResponse
                    {
                        Message = "���N�G�X�g�f�[�^����ł�"
                    });
                }

                ConvertRequest? request;
                try
                {
                    request = JsonSerializer.Deserialize<ConvertRequest>(requestBody, GetJsonSerializerOptions());
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning("[{RequestId}] JSON��̓G���[: {Error}", requestId, ex.Message);
                    return await CreateErrorResponse(req, HttpStatusCode.BadRequest, new ErrorResponse
                    {
                        Message = "JSON�t�H�[�}�b�g���s���ł�",
                        Details = ex.Message
                    });
                }

                if (request == null)
                {
                    return await CreateErrorResponse(req, HttpStatusCode.BadRequest, new ErrorResponse
                    {
                        Message = "���N�G�X�g�f�[�^��null�ł�"
                    });
                }

                _logger.LogInformation("[{RequestId}] PDF�ϊ����N�G�X�g�J�n: User={UserId}, File={FileName}, Size={FileSize}bytes",
                    requestId, request.UserId, request.FileName, request.FileData?.Length ?? 0);

                // ���͒l���؁iTryValidateModel�̑�ցj
                var validationResults = new List<ValidationResult>();
                var validationContext = new ValidationContext(request);

                if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
                {
                    var errors = validationResults.Select(x => x.ErrorMessage);
                    var errorMessage = string.Join(", ", errors);
                    _logger.LogWarning("[{RequestId}] ���͒l���؃G���[: {Errors}", requestId, errorMessage);

                    return await CreateErrorResponse(req, HttpStatusCode.BadRequest, new ErrorResponse
                    {
                        Message = "���͒l���s���ł�",
                        Details = errorMessage
                    });
                }

                // �J�X�^������
                var validationResult = ValidateRequest(request);
                if (!validationResult.IsValid)
                {
                    _logger.LogWarning("[{RequestId}] �J�X�^�����؃G���[: {Error}", requestId, validationResult.ErrorMessage);
                    return await CreateErrorResponse(req, HttpStatusCode.BadRequest, new ErrorResponse
                    {
                        Message = "���N�G�X�g�f�[�^���s���ł�",
                        Details = validationResult.ErrorMessage
                    });
                }

                try
                {
                    // PDF�ϊ��������s
                    var response = await ProcessPdfConversionAsync(request, requestId);
                    _logger.LogInformation("[{RequestId}] PDF�ϊ�����: OutputFile={OutputFileName}, OutputSize={OutputSize}bytes",
                        requestId, response.FileName, response.FileData.Length);

                    return await CreateJsonResponse(req, HttpStatusCode.OK, response);
                }
                catch (PdfReaderException ex) when (ex.Message.Contains("�p�X���[�h") || ex.Message.Contains("password"))
                {
                    // �p�X���[�h�ی삳�ꂽPDF�̏���
                    _logger.LogWarning("[{RequestId}] �p�X���[�h�ی삳�ꂽPDF: {Message}", requestId, ex.Message);
                    return await CreateErrorResponse(req, HttpStatusCode.BadRequest, new ErrorResponse
                    {
                        Message = "�p�X���[�h���ݒ肳��Ă��܂��B",
                        Details = ex.Message
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[{RequestId}] PDF�ϊ������G���[", requestId);
                    return await CreateErrorResponse(req, HttpStatusCode.BadRequest, new ErrorResponse
                    {
                        Message = "�p�X���[�h�ݒ菈���Ɏ��s���܂����B",
                        Details = ex.Message
                    });
                }
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "[{RequestId}] �����G���[", requestId);
                return await CreateErrorResponse(req, HttpStatusCode.BadRequest, new ErrorResponse
                {
                    Message = "�s���ȃ��N�G�X�g�ł�",
                    Details = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "[{RequestId}] �����G���[", requestId);
                return await CreateErrorResponse(req, HttpStatusCode.UnprocessableEntity, new ErrorResponse
                {
                    Message = "���������s�ł��܂���ł���",
                    Details = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{RequestId}] �\�����Ȃ��G���[", requestId);
                return await CreateErrorResponse(req, HttpStatusCode.InternalServerError, new ErrorResponse
                {
                    Message = "�����T�[�o�[�G���[���������܂���"
                });
            }
        }

        /// <summary>
        /// ���N�G�X�g�f�[�^�̃J�X�^������
        /// </summary>
        private (bool IsValid, string ErrorMessage) ValidateRequest(ConvertRequest request)
        {
            // Null �`�F�b�N
            if (request.FileData == null)
            {
                return (false, "�t�@�C���f�[�^��null�ł�");
            }

            // �t�@�C���T�C�Y�`�F�b�N
            if (request.FileData.Length > MAX_FILE_SIZE)
            {
                return (false, $"�t�@�C���T�C�Y������({MAX_FILE_SIZE / (1024 * 1024)}MB)�𒴂��Ă��܂�");
            }

            // PDF�t�@�C���`���`�F�b�N
            if (!IsPdfFile(request.FileData))
            {
                return (false, "PDF�t�@�C���ł͂���܂���");
            }

            // �t�@�C�����̈��S���`�F�b�N
            if (string.IsNullOrWhiteSpace(request.FileName))
            {
                return (false, "�t�@�C��������ł�");
            }

            if (HasInvalidFileNameChars(request.FileName))
            {
                return (false, "�t�@�C�����Ɏg�p�ł��Ȃ��������܂܂�Ă��܂�");
            }

            return (true, string.Empty);
        }

        /// <summary>
        /// PDF�t�@�C������
        /// </summary>
        private static bool IsPdfFile(byte[] fileData)
        {
            if (fileData.Length < 5) return false;

            var header = System.Text.Encoding.ASCII.GetString(fileData, 0, 5);
            return header.Equals("%PDF-", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// �t�@�C�����̈��S���`�F�b�N
        /// </summary>
        private static bool HasInvalidFileNameChars(string fileName)
        {
            return fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0;
        }

        /// <summary>
        /// PDF�ϊ�����
        /// </summary>
        private async Task<ConvertResponse> ProcessPdfConversionAsync(ConvertRequest request, string requestId)
        {
            // �������Ԃ̃V�~�����[�V�����i���̃R�[�h����6�b��1.5�b�ɕύX�j
            await Task.Delay(1500);

            _logger.LogInformation("[{RequestId}] : �����J�n", requestId);

            try
            {
                // PdfPasswordProtector���g�p���ăp�X���[�h�ݒ�
                PdfPasswordProtector pdfPasswordProtector = new PdfPasswordProtector();
                var passPdf = pdfPasswordProtector.SetPassword(request.FileData, request.ViewPassword);

                _logger.LogInformation("[{RequestId}] : �����I��", requestId);

                // ������t�@�C��������
                var processedFileName = GenerateProcessedFileName(request.FileName);

                return new ConvertResponse
                {
                    FileName = processedFileName,
                    FileData = passPdf
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{RequestId}] PDF�ϊ������ŃG���[������", requestId);
                throw; // �O����catch�߂œK�؂Ƀn���h�����O�����
            }
        }

        /// <summary>
        /// ������t�@�C��������
        /// </summary>
        private static string GenerateProcessedFileName(string originalFileName)
        {
            var nameWithoutExt = Path.GetFileNameWithoutExtension(originalFileName);
            var extension = Path.GetExtension(originalFileName);
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

            return $"{nameWithoutExt}_converted_{timestamp}{extension}";
        }

        /// <summary>
        /// JSON �V���A���C�U�I�v�V����
        /// </summary>
        private JsonSerializerOptions GetJsonSerializerOptions()
        {
            return new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                PropertyNameCaseInsensitive = true
            };
        }

        /// <summary>
        /// JSON ���X�|���X�쐬�w���p�[
        /// </summary>
        private async Task<HttpResponseData> CreateJsonResponse<T>(HttpRequestData req, HttpStatusCode statusCode, T data)
        {
            var response = req.CreateResponse(statusCode);
            response.Headers.Add("Content-Type", "application/json");

            await response.WriteStringAsync(JsonSerializer.Serialize(data, GetJsonSerializerOptions()));
            return response;
        }

        /// <summary>
        /// �G���[���X�|���X�쐬�w���p�[
        /// </summary>
        private async Task<HttpResponseData> CreateErrorResponse(HttpRequestData req, HttpStatusCode statusCode, ErrorResponse error)
        {
            return await CreateJsonResponse(req, statusCode, error);
        }
    }
}