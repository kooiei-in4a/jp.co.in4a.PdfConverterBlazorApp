using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfConverterShare.Models
{
    /// <summary>
    /// API関連の設定クラス
    /// </summary>
    public class ApiSettings
    {
        /// <summary>
        /// APIのベースURL
        /// </summary>
        public string BaseUrl { get; set; } = string.Empty;

        /// <summary>
        /// タイムアウト時間（秒）
        /// </summary>
        public int TimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// 最大リトライ回数
        /// </summary>
        public int MaxRetryAttempts { get; set; } = 3;

        /// <summary>
        /// 設定の妥当性をチェック
        /// </summary>
        /// <returns>設定が有効かどうか</returns>
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(BaseUrl) &&
                   Uri.IsWellFormedUriString(BaseUrl, UriKind.Absolute) &&
                   TimeoutSeconds > 0 &&
                   MaxRetryAttempts >= 0;
        }
    }
}
