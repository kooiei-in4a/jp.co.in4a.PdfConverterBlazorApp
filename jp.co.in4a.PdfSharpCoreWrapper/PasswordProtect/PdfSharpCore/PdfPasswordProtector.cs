using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;
using PdfSharpCore.Pdf.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jp.co.in4a.PdfSharpCoreWrapper.PasswordProtect.PdfSharpCore
{
    public class PdfPasswordProtector : IPdfPasswordProtector
    {
        public byte[] SetPassword(byte[] sourceBin, string password)
        {
            // PDFドキュメントを開く
            var document = OpenPdf(sourceBin);

            // セキュリティが掛かってないかをチェック
            if (IsNoPassword(document))
            {
                // パスワードを設定する
                document.SecuritySettings.DocumentSecurityLevel = PdfDocumentSecurityLevel.Encrypted128Bit;
                document.SecuritySettings.UserPassword = password;
                document.SecuritySettings.OwnerPassword = password;

                // 4. 新しいメモリストリームに変更後のPDFを保存する
                using (var outputStream = new MemoryStream())
                {
                    document.Save(outputStream, false);
                    document.Close();

                    // 5. バイナリデータを返す
                    return outputStream.ToArray();
                }
            }

            throw new NotImplementedException();
        }

        public PdfDocument OpenPdf(byte[] pdfBinary)
        {
            using (var inputStream = new MemoryStream(pdfBinary))
            {
                // パスワード無で開く
                inputStream.Position = 0;
                var doc = PdfReader.Open(inputStream);
                return doc;
            }
        }

        public bool IsNoPassword(PdfDocument document) { 

            if (document.SecuritySettings.DocumentSecurityLevel == PdfDocumentSecurityLevel.None)
            {
                return true;
            }

            return false;
            
        }

    }
}
