using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;
using PdfSharpCore.Pdf.IO.enums;
using SixLabors.Fonts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jp.co.in4a.PdfSharpCoreWrapper
{
    public class Utils
    {

        private static void PdfBinCheck(byte[] pdfBinary)
        {
            // 入力検証
            if (pdfBinary == null || pdfBinary.Length == 0)
                throw new ArgumentException("PDFバイナリデータが無効です。", nameof(pdfBinary));
            // PDFヘッダーの簡易チェック
            if (pdfBinary.Length < 8 || !System.Text.Encoding.ASCII.GetString(pdfBinary, 0, 4).Equals("%PDF"))
                throw new ArgumentException("有効なPDFファイルではありません。", nameof(pdfBinary));
        }

        public static bool IsViewingPasswordProtected(byte[] pdfBinary)
        {
            PdfBinCheck(pdfBinary); // 入力検証は重要
            try
            {
                using (var stream = new MemoryStream(pdfBinary))
                {
                    // InformationOnlyモードで最速チェック
                    PdfReader.Open(stream, PdfDocumentOpenMode.InformationOnly);
                }
                return false;
            }
            catch (PdfReaderException ex)
            {
                // パスワードが原因の例外かを確認
                return ex.Message.IndexOf("password", StringComparison.OrdinalIgnoreCase) >= 0;
            }
            // その他の例外は上位に投げるべきなので、ここではキャッチしない方がシンプル


            //PdfSecurityInfo pdfSecurityInfo = new PdfSecurityInfo();

            //try
            //{
            //    OpenPdfFile(pdfBinary, null, PdfDocumentOpenMode.ReadOnly, PdfReadAccuracy.Strict);
            //    return false;
            //}
            //catch (PdfReaderException ex)
            //{
            //    // パスワード保護されている場合は例外が発生する
            //    if (ex.Message.Contains("パスワード") || ex.Message.Contains("password"))
            //    {
            //        return true;
            //    }
            //    else
            //    {
            //        throw;
            //    }
            //}
        }

        public static bool IsPermissionPasswordProtected(byte[] pdfBinary, string viewingPasswordProtected)
        {
            PdfSecurityInfo pdfSecurityInfo = new PdfSecurityInfo();
            try
            {
                OpenPdfFile(pdfBinary, viewingPasswordProtected, PdfDocumentOpenMode.Modify, PdfReadAccuracy.Strict);
                return false;
            }
            catch (PdfReaderException ex)
            {
                if (ex.Message.Contains("パスワード") || ex.Message.Contains("password"))
                {
                    return true;
                }
                else
                {
                    throw;
                }
            }
        }


        public static PdfSecurityInfo IsPasswordProtected(byte[] pdfBinary,string viewingPasswordProtected)
        {
            PdfSecurityInfo pdfSecurityInfo = new PdfSecurityInfo();
            
            pdfSecurityInfo.isViewingPasswordProtected = IsViewingPasswordProtected(pdfBinary);
            // 閲覧パスワードが必要な場合は、パスワードを指定して開く
            pdfSecurityInfo.isPermissionPasswordProtected = IsPermissionPasswordProtected(pdfBinary, viewingPasswordProtected);
           
            return pdfSecurityInfo;

        }
        public static PdfDocument OpenPdfFile(byte[] pdfBinary)
        {
            return OpenPdfFile(pdfBinary, null, null, null);
        }

        public static PdfDocument OpenPdfFile(byte[] pdfBinary,string password)
        {
            return OpenPdfFile(pdfBinary, password, null, null);
        }

        public static PdfDocument OpenPdfFile(
                byte[] pdfBinary,
                string? password,
                PdfDocumentOpenMode? pdfDocumentOpenMode = null,
                PdfReadAccuracy? pdfReadAccuracy = null)
        {
            // PDFバイナリの検証
            PdfBinCheck(pdfBinary);

            PdfDocumentOpenMode openMode = pdfDocumentOpenMode ?? PdfDocumentOpenMode.Modify;
            PdfReadAccuracy readAccuracy = pdfReadAccuracy ?? PdfReadAccuracy.Strict;


            using (var inputStream = new MemoryStream(pdfBinary))
            {
                while (true)
                {
                    try
                    {
                        inputStream.Position = 0;
                        var doc = PdfReader.Open(inputStream, password, openMode, readAccuracy);
                        return doc;
                    }
                    catch (PdfReaderException)
                    {
                        // 失敗したログを出力
                        Console.WriteLine("Open失敗 PdfDocumentOpenMode:{0} , PdfReadAccuracy:{1}",openMode.ToString(),readAccuracy.ToString());

                        // 引数で指定されたオープンモードと読み取り精度で開けなかった場合は例外とする
                        if (pdfDocumentOpenMode == openMode && pdfReadAccuracy == readAccuracy)
                        {
                            throw;
                        }

                        // オープンモードを変更して再試行
                        if (pdfDocumentOpenMode == null)
                        {
                            switch (openMode)
                            {
                                case PdfDocumentOpenMode.Import:
                                    openMode = PdfDocumentOpenMode.ReadOnly;
                                    continue;
                                case PdfDocumentOpenMode.ReadOnly:
                                    openMode = PdfDocumentOpenMode.Modify;
                                    continue;
                                case PdfDocumentOpenMode.Modify:
                                    openMode = PdfDocumentOpenMode.Import;
                                    break;
                                case PdfDocumentOpenMode.InformationOnly:
                                default:
                                    throw new ArgumentException("無効なPDFドキュメントオープンモードです。", nameof(pdfDocumentOpenMode));
                            }
                        }

                        // 読み取り精度を変更して再試行
                        if (pdfReadAccuracy == null && readAccuracy == PdfReadAccuracy.Strict)
                        {
                            readAccuracy = PdfReadAccuracy.Moderate;
                            continue;
                        }

                        // ここまで来た場合は、オープンモードと読み取り精度の組み合わせで開けなかったことを示す
                        throw;

                    }
                    catch (OutOfMemoryException ex)
                    {
                        throw new InvalidOperationException("PDFファイルが大きすぎてメモリに読み込めません。", ex);
                    }
                    catch (IOException ex)
                    {
                        throw new InvalidOperationException("PDFファイルの処理中にI/Oエラーが発生しました。", ex);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        throw; // 既に適切な例外なので再スロー
                    }
                    catch (ArgumentException)
                    {
                        throw; // 既に適切な例外なので再スロー
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException("パスワード解除処理中に予期しないエラーが発生しました。", ex);
                    }

                }
            }
        }
    }
}