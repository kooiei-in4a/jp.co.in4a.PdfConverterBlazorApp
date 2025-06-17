using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PdfSharpCore;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;
using PdfSharpCore.Pdf.Security;

namespace jp.co.in4a.PdfSharpCoreWrapper
{
    public class PasswordRemover
    {



        /// <summary>
        /// PDFのパスワードを解除します。オーナーパスワードを知っている場合に使えます。
        /// </summary>
        /// <param name="pdfBinary"></param>
        /// <param name="password">オーナーパスワード</param>
        /// <returns></returns>
        /// <exception cref="PdfReaderException"></exception>
        public static byte[] RemovePdfPassword(byte[] pdfBinary, string password) 
        {
            var srcDoc= Utils.OpenPdfFile(pdfBinary, password,PdfDocumentOpenMode.Modify);
            try
            {
                // パスワード保護を解除したPDFを出力
                using (var outputStream = new MemoryStream())
                {

                    srcDoc.Save(outputStream, false);
                    var srcBin = outputStream.ToArray();

                    var targetDoc = Utils.OpenPdfFile(srcBin,password,PdfDocumentOpenMode.Modify);
                    if (targetDoc.SecuritySettings.DocumentSecurityLevel == PdfDocumentSecurityLevel.None)
                    {
                        throw new PdfReaderException("PDFはパスワード保護されていません。");
                    }

                    return srcBin;
                        var securitySettings = targetDoc.SecuritySettings;

                    //var result = new PdfSecurityInfo();

                    ////【確定ロジック】DocumentSecurityLevelでセキュリティの有無を判断
                    //if (securitySettings.DocumentSecurityLevel == PdfDocumentSecurityLevel.None)
                    //{
                    //    result.SecurityMethod = "セキュリティなし";
                    //    result.HasDocumentOpenPassword = "無";
                    //    result.HasPermissionPassword = "無";
                    //    result.Printing = "許可";
                    //    result.DocumentChanges = "許可";
                    //    result.Annotations = "許可";
                    //    result.FormFieldFilling = "許可";
                    //    result.DocumentAssembly = "許可";
                    //    result.ContentCopying = "許可";
                    //    result.Accessibility = "許可";
                    //    result.EncryptionLevel = "なし";
                    //}

                }
            }
            finally
            {
                srcDoc.Close();
            }
        }


    }
}
