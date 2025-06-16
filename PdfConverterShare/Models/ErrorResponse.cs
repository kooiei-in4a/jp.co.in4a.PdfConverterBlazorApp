﻿namespace PdfConverterShare.Models
{
    /// <summary>
    /// エラーレスポンスモデル
    /// </summary>
    public class ErrorResponse
    {
        public string Message { get; set; } = string.Empty;
        public string? Details { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
