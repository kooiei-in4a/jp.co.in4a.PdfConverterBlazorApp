namespace PdfConverterApi.Models
{
    /// <summary>
    /// CORS設定を管理するクラス
    /// </summary>
    public class CorsSettings
    {
        /// <summary>
        /// CORSポリシー名
        /// </summary>
        public string PolicyName { get; set; } = "AllowBlazorWasm";

        /// <summary>
        /// 許可されたオリジン
        /// </summary>
        public string[] AllowedOrigins { get; set; } = Array.Empty<string>();

        /// <summary>
        /// 全てのヘッダーを許可するか
        /// </summary>
        public bool AllowAnyHeader { get; set; } = true;

        /// <summary>
        /// 全てのHTTPメソッドを許可するか
        /// </summary>
        public bool AllowAnyMethod { get; set; } = true;

        /// <summary>
        /// レスポンスで公開するヘッダー
        /// </summary>
        public string[] ExposedHeaders { get; set; } = Array.Empty<string>();
    }
}