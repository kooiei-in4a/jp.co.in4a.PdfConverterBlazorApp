namespace PdfConverterShare.Models
{
    /// <summary>
    /// API情報レスポンスモデル
    /// </summary>
    public class ApiInfoResponse
    {
        public string Service { get; set; } = "PDF変換API";
        public string Version { get; set; } = "1.0.0";
        public string Status { get; set; } = "正常";
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
