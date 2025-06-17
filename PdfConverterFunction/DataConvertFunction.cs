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
        /// API動作確認
        /// </summary>
        [Function("GetApiInfo")]
        public async Task<HttpResponseData> GetApiInfo(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "api/dataconvert")] HttpRequestData req)
        {
            _logger.LogInformation("API動作確認リクエスト");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json");

            var apiInfo = new ApiInfoResponse();
            await response.WriteStringAsync(JsonSerializer.Serialize(apiInfo, GetJsonSerializerOptions()));

            return response;
        }

        /// <summary>
        /// PDFファイル変換処理
        /// </summary>
        [Function("ConvertPdf")]
        public async Task<HttpResponseData> ConvertPdf(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "api/dataconvert")] HttpRequestData req)
        {
            var requestId = Guid.NewGuid().ToString("N")[..8];

            try
            {
                // リクエストサイズログ
                _logger.LogInformation("[{RequestId}] PDF変換リクエスト開始: Size={FileSize}bytes",requestId, req.Body.Length);


                // リクエストサイズチェック（RequestSizeLimitの代替）
                if (req.Body.Length > MAX_FILE_SIZE)
                {
                    _logger.LogWarning("[{RequestId}] ファイルサイズ制限超過: {Size}bytes", requestId, req.Body.Length);
                    return await CreateErrorResponse(req, HttpStatusCode.BadRequest, new ErrorResponse
                    {
                        Message = "ファイルサイズが制限を超えています",
                        Details = $"最大サイズ: {MAX_FILE_SIZE / (1024 * 1024)}MB"
                    });
                }

                // リクエストボディの読み取り（[FromBody]の代替）
                string requestBody = await req.ReadAsStringAsync() ?? "";

                if (string.IsNullOrEmpty(requestBody))
                {
                    _logger.LogWarning("[{RequestId}] リクエストボディが空です", requestId);
                    return await CreateErrorResponse(req, HttpStatusCode.BadRequest, new ErrorResponse
                    {
                        Message = "リクエストデータが空です"
                    });
                }

                ConvertRequest? request;
                try
                {
                    request = JsonSerializer.Deserialize<ConvertRequest>(requestBody, GetJsonSerializerOptions());
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning("[{RequestId}] JSON解析エラー: {Error}", requestId, ex.Message);
                    return await CreateErrorResponse(req, HttpStatusCode.BadRequest, new ErrorResponse
                    {
                        Message = "JSONフォーマットが不正です",
                        Details = ex.Message
                    });
                }

                if (request == null)
                {
                    return await CreateErrorResponse(req, HttpStatusCode.BadRequest, new ErrorResponse
                    {
                        Message = "リクエストデータがnullです"
                    });
                }

                _logger.LogInformation("[{RequestId}] PDF変換リクエスト開始: User={UserId}, File={FileName}, Size={FileSize}bytes",
                    requestId, request.UserId, request.FileName, request.FileData?.Length ?? 0);

                // 入力値検証（TryValidateModelの代替）
                var validationResults = new List<ValidationResult>();
                var validationContext = new ValidationContext(request);

                if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
                {
                    var errors = validationResults.Select(x => x.ErrorMessage);
                    var errorMessage = string.Join(", ", errors);
                    _logger.LogWarning("[{RequestId}] 入力値検証エラー: {Errors}", requestId, errorMessage);

                    return await CreateErrorResponse(req, HttpStatusCode.BadRequest, new ErrorResponse
                    {
                        Message = "入力値が不正です",
                        Details = errorMessage
                    });
                }

                // カスタム検証
                var validationResult = ValidateRequest(request);
                if (!validationResult.IsValid)
                {
                    _logger.LogWarning("[{RequestId}] カスタム検証エラー: {Error}", requestId, validationResult.ErrorMessage);
                    return await CreateErrorResponse(req, HttpStatusCode.BadRequest, new ErrorResponse
                    {
                        Message = "リクエストデータが不正です",
                        Details = validationResult.ErrorMessage
                    });
                }

                try
                {
                    // PDF変換処理実行
                    var response = await ProcessPdfConversionAsync(request, requestId);
                    _logger.LogInformation("[{RequestId}] PDF変換完了: OutputFile={OutputFileName}, OutputSize={OutputSize}bytes",
                        requestId, response.FileName, response.FileData.Length);

                    return await CreateJsonResponse(req, HttpStatusCode.OK, response);
                }
                catch (PdfReaderException ex) when (ex.Message.Contains("パスワード") || ex.Message.Contains("password"))
                {
                    // パスワード保護されたPDFの処理
                    _logger.LogWarning("[{RequestId}] パスワード保護されたPDF: {Message}", requestId, ex.Message);
                    return await CreateErrorResponse(req, HttpStatusCode.BadRequest, new ErrorResponse
                    {
                        Message = "パスワードが設定されています。",
                        Details = ex.Message
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[{RequestId}] PDF変換処理エラー", requestId);
                    return await CreateErrorResponse(req, HttpStatusCode.BadRequest, new ErrorResponse
                    {
                        Message = "パスワード設定処理に失敗しました。",
                        Details = ex.Message
                    });
                }
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "[{RequestId}] 引数エラー", requestId);
                return await CreateErrorResponse(req, HttpStatusCode.BadRequest, new ErrorResponse
                {
                    Message = "不正なリクエストです",
                    Details = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "[{RequestId}] 処理エラー", requestId);
                return await CreateErrorResponse(req, HttpStatusCode.UnprocessableEntity, new ErrorResponse
                {
                    Message = "処理を実行できませんでした",
                    Details = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{RequestId}] 予期しないエラー", requestId);
                return await CreateErrorResponse(req, HttpStatusCode.InternalServerError, new ErrorResponse
                {
                    Message = "内部サーバーエラーが発生しました"
                });
            }
        }

        /// <summary>
        /// リクエストデータのカスタム検証
        /// </summary>
        private (bool IsValid, string ErrorMessage) ValidateRequest(ConvertRequest request)
        {
            // Null チェック
            if (request.FileData == null)
            {
                return (false, "ファイルデータがnullです");
            }

            // ファイルサイズチェック
            if (request.FileData.Length > MAX_FILE_SIZE)
            {
                return (false, $"ファイルサイズが制限({MAX_FILE_SIZE / (1024 * 1024)}MB)を超えています");
            }

            // PDFファイル形式チェック
            if (!IsPdfFile(request.FileData))
            {
                return (false, "PDFファイルではありません");
            }

            // ファイル名の安全性チェック
            if (string.IsNullOrWhiteSpace(request.FileName))
            {
                return (false, "ファイル名が空です");
            }

            if (HasInvalidFileNameChars(request.FileName))
            {
                return (false, "ファイル名に使用できない文字が含まれています");
            }

            return (true, string.Empty);
        }

        /// <summary>
        /// PDFファイル判定
        /// </summary>
        private static bool IsPdfFile(byte[] fileData)
        {
            if (fileData.Length < 5) return false;

            var header = System.Text.Encoding.ASCII.GetString(fileData, 0, 5);
            return header.Equals("%PDF-", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// ファイル名の安全性チェック
        /// </summary>
        private static bool HasInvalidFileNameChars(string fileName)
        {
            return fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0;
        }

        /// <summary>
        /// PDF変換処理
        /// </summary>
        private async Task<ConvertResponse> ProcessPdfConversionAsync(ConvertRequest request, string requestId)
        {
            // 処理時間のシミュレーション（元のコードから6秒を1.5秒に変更）
            await Task.Delay(1500);

            _logger.LogInformation("[{RequestId}] : 処理開始", requestId);

            try
            {
                // PdfPasswordProtectorを使用してパスワード設定
                PdfPasswordProtector pdfPasswordProtector = new PdfPasswordProtector();
                var passPdf = pdfPasswordProtector.SetPassword(request.FileData, request.ViewPassword);

                _logger.LogInformation("[{RequestId}] : 処理終了", requestId);

                // 処理後ファイル名生成
                var processedFileName = GenerateProcessedFileName(request.FileName);

                return new ConvertResponse
                {
                    FileName = processedFileName,
                    FileData = passPdf
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{RequestId}] PDF変換処理でエラーが発生", requestId);
                throw; // 外側のcatch節で適切にハンドリングされる
            }
        }

        /// <summary>
        /// 処理後ファイル名生成
        /// </summary>
        private static string GenerateProcessedFileName(string originalFileName)
        {
            var nameWithoutExt = Path.GetFileNameWithoutExtension(originalFileName);
            var extension = Path.GetExtension(originalFileName);
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

            return $"{nameWithoutExt}_converted_{timestamp}{extension}";
        }

        /// <summary>
        /// JSON シリアライザオプション
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
        /// JSON レスポンス作成ヘルパー
        /// </summary>
        private async Task<HttpResponseData> CreateJsonResponse<T>(HttpRequestData req, HttpStatusCode statusCode, T data)
        {
            var response = req.CreateResponse(statusCode);
            response.Headers.Add("Content-Type", "application/json");

            await response.WriteStringAsync(JsonSerializer.Serialize(data, GetJsonSerializerOptions()));
            return response;
        }

        /// <summary>
        /// エラーレスポンス作成ヘルパー
        /// </summary>
        private async Task<HttpResponseData> CreateErrorResponse(HttpRequestData req, HttpStatusCode statusCode, ErrorResponse error)
        {
            return await CreateJsonResponse(req, statusCode, error);
        }
    }
}