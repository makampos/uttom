using System.Text.RegularExpressions;
using Uttom.Domain.Interfaces.Abstractions;

namespace Uttom.Domain.Interfaces.Services;
// TODO: Add a wrapper response instead a validation boolean directly
public class ImageService : IImageService
{
    public bool ValidateImageExtension(string base64ImageData)
    {
        if (string.IsNullOrEmpty(base64ImageData))
        {
            return false;
        }

        // Remove the data URL scheme if present
        var base64Data = base64ImageData.Contains(",")
            ? base64ImageData.Split(',')[1]
            : base64ImageData;

        // Check if the base64 string is valid
        if (!IsBase64String(base64Data))
        {
            return false;
        }

        //TODO: Keep extension of the file

        // Check for PNG or BMP signatures in the first few bytes
        // PNG: 89 50 4E 47 0D 0A 1A 0A (base64 starts with "iVBORw")
        // BMP: 42 4D (base64 starts with "Qk")
        return base64Data.StartsWith("iVBORw") || base64Data.StartsWith("Qk");
    }

    private bool IsBase64String(string base64)
    {
        // Check if the string is a valid Base64 string
        return (base64.Length % 4 == 0) &&
               Regex.IsMatch(base64, @"^[a-zA-Z0-9+/]*={0,2}$");
    }
}