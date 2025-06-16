using System.ComponentModel.DataAnnotations;

namespace PdfConverterShare.Models
{
    /// <summary>
    /// PDF変換リクエストモデル
    /// </summary>
    public class ConvertRequest
    {
        [Required(ErrorMessage = "ユーザーIDは必須です")]
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "ファイル名は必須です")]
        public string FileName { get; set; } = string.Empty;

        [Required(ErrorMessage = "参照用パスワードは必須です")]
        public string ViewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "編集用パスワードは必須です")]
        public string EditPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "ファイルデータは必須です")]
        public byte[] FileData { get; set; } = Array.Empty<byte>();
    }
}
