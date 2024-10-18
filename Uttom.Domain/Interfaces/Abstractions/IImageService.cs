namespace Uttom.Domain.Interfaces.Abstractions;

public interface IImageService
{
    bool ValidateImageExtension(string base64ImageData);
}