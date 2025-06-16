using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using PdfConverterShare.Models;
using Microsoft.AspNetCore.RateLimiting;

namespace PdfConverterApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [EnableRateLimiting("minute")]
    public class DataConvertController : ControllerBase
    {
        private readonly ILogger<DataConvertController> _logger;
        private const int MAX_FILE_SIZE = 20 * 1024 * 1024; // 20MB

        public DataConvertController(ILogger<DataConvertController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// PDFファイル変換処理
        /// </summary>
        [HttpPost]
        [RequestSizeLimit(MAX_FILE_SIZE)]
        public async Task<ActionResult<ConvertResponse>> Post([FromBody] ConvertRequest request)
        {
            var requestId = Guid.NewGuid().ToString("N")[..8];

            try
            {
                _logger.LogInformation("[{RequestId}] PDF変換リクエスト開始: User={UserId}, File={FileName}, Size={FileSize}bytes",
                    requestId, request.UserId, request.FileName, request.FileData?.Length ?? 0);

                // 入力値検証
                if (!TryValidateModel(request))
                {
                    var errors = ModelState
                        .Where(x => x.Value?.Errors.Count > 0)
                        .SelectMany(x => x.Value!.Errors)
                        .Select(x => x.ErrorMessage);

                    var errorMessage = string.Join(", ", errors);
                    _logger.LogWarning("[{RequestId}] 入力値検証エラー: {Errors}", requestId, errorMessage);

                    return BadRequest(new ErrorResponse
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
                    return BadRequest(new ErrorResponse
                    {
                        Message = "リクエストデータが不正です",
                        Details = validationResult.ErrorMessage
                    });
                }

                // PDF変換処理実行
                var response = await ProcessPdfConversionAsync(request, requestId);

                _logger.LogInformation("[{RequestId}] PDF変換完了: OutputFile={OutputFileName}, OutputSize={OutputSize}bytes",
                    requestId, response.FileName, response.FileData.Length);

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "[{RequestId}] 引数エラー", requestId);
                return BadRequest(new ErrorResponse
                {
                    Message = "不正なリクエストです",
                    Details = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "[{RequestId}] 処理エラー", requestId);
                return UnprocessableEntity(new ErrorResponse
                {
                    Message = "処理を実行できませんでした",
                    Details = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{RequestId}] 予期しないエラー", requestId);
                return StatusCode(500, new ErrorResponse
                {
                    Message = "内部サーバーエラーが発生しました"
                });
            }
        }

        /// <summary>
        /// API動作確認
        /// </summary>
        [HttpGet]
        public ActionResult<ApiInfoResponse> Get()
        {
            _logger.LogInformation("API動作確認リクエスト");

            return Ok(new ApiInfoResponse());
        }

        /// <summary>
        /// リクエストデータのカスタム検証
        /// </summary>
        private (bool IsValid, string ErrorMessage) ValidateRequest(ConvertRequest request)
        {
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
        /// PDF変換処理（ダミー実装）
        /// </summary>
        private async Task<ConvertResponse> ProcessPdfConversionAsync(ConvertRequest request, string requestId)
        {
            // ダミー処理: 実際の変換処理のシミュレーション
            await Task.Delay(6000); // 1.5秒の処理時間

            _logger.LogInformation("[{RequestId}] ダミー変換処理実行: パスワード設定完了", requestId);

            // 処理後ファイル名生成
            var processedFileName = GenerateProcessedFileName(request.FileName);

            return new ConvertResponse
            {
                FileName = processedFileName,
                FileData = request.FileData // ダミー: 元データをそのまま返却
            };
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
    }
}
