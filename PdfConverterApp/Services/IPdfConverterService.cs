using PdfConverterShare.Models;

namespace PdfConverterApp.Services
{
    public interface IPdfConverterService
    {
        Task<ConvertResponse?> ConvertPdfAsync(ConvertRequest request);
        Task<bool> CheckApiHealthAsync();
    }
}
