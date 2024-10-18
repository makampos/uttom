namespace Uttom.Application.Extensions;

public static class StringExtensions
{
    public static string ConvertToBase64(string filePath)
    {
        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), filePath);

        var imageBytes = File.ReadAllBytes(fullPath);
        return Convert.ToBase64String(imageBytes);
    }
}