using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jp.co.in4a.PdfSharpCoreWrapper.PasswordProtect
{
    /// <summary>
    /// PDFドキュメントにパスワードを設定するインターフェース
    /// </summary>
    public interface IPdfPasswordProtector
    {
        /// <summary>
        /// PDFドキュメントにパスワードを設定する
        /// </summary>
        /// <param name="document">PDFドキュメントオブジェクト</param>
        /// <param name="password">設定するパスワード</param>
        byte[] SetPassword(byte[] sourceBin, string password);
    }
}
