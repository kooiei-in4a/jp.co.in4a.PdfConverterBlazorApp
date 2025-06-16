namespace PdfConverterShare.Models
{
    /// <summary>
    /// PDF変換レスポンスモデル
    /// </summary>
    public class ConvertResponse
    {
        public string FileName { get; set; } = string.Empty;
        public byte[] FileData { get; set; } = Array.Empty<byte>();
    }
}
