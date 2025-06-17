using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jp.co.in4a.PdfSharpCoreWrapper
{
    public class PdfSecurityInfo
    {
        public string SecurityMethod { get; set; } = "不明";
        public string HasDocumentOpenPassword { get; set; } = "不明";
        public string HasPermissionPassword { get; set; } = "不明";
        public string Printing { get; set; } = "不明";
        public string DocumentChanges { get; set; } = "不明";
        public string Annotations { get; set; } = "不明";
        public string FormFieldFilling { get; set; } = "不明";
        public string DocumentAssembly { get; set; } = "不明";
        public string ContentCopying { get; set; } = "不明";
        public string Accessibility { get; set; } = "不明";
        public string EncryptionLevel { get; set; } = "なし";


        public bool isViewingPasswordProtected { get; set; } = false;

        public bool isPermissionPasswordProtected { get; set; } = false;
    }
}
