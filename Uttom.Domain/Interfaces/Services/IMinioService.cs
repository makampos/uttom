namespace Uttom.Domain.Interfaces.Services;

public interface IMinioService
{
    Task<string> UploadImageAsync(int delivererId, string base64ImageData);
    Task<MemoryStream> GetImageAsync(string objectName);
}