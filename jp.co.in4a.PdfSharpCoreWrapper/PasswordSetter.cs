using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;
using PdfSharpCore.Pdf.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace jp.co.in4a.PdfSharpCoreWrapper
{
    public class PasswordSetter
    {
        public static byte[] SetPdfPassword(byte[] pdfBinary, string password)
        {
            var document = Utils.OpenPdfFile(pdfBinary, null, PdfDocumentOpenMode.Modify);

            try
            {
                document.SecuritySettings.UserPassword = password;

                // 4. 新しいメモリストリームに変更後のPDFを保存する
                using (var outputStream = new MemoryStream())
                {
                    document.Save(outputStream,false);
                    document.Close();

                    // 5. バイナリデータを返す
                    return outputStream.ToArray();
                }
            }
            finally
            {
                document.Close();
            }
        }

        public static byte[] SetPdfPassword(byte[] pdfBinary, string userPassword,string ownerPassword)
        {
            var document = Utils.OpenPdfFile(pdfBinary, null, PdfDocumentOpenMode.Modify);

            try
            {
                document.SecuritySettings.DocumentSecurityLevel = PdfDocumentSecurityLevel.Encrypted128Bit;
                document.SecuritySettings.UserPassword = userPassword;
                document.SecuritySettings.OwnerPassword = ownerPassword;

                // 4. 新しいメモリストリームに変更後のPDFを保存する
                using (var outputStream = new MemoryStream())
                {
                    document.Save(outputStream, false);
                    document.Close();

                    // 5. バイナリデータを返す
                    return outputStream.ToArray();
                }
            }catch (Exception ex)
            {
                // エラーハンドリング: 例外をログに記録するか、適切な処理を行う
                Console.WriteLine($"Error setting PDF password: {ex.Message}");
                throw; // 必要に応じて例外を再スローする
            }
            finally
            {
                document.Close();
            }
        }

    }
}
