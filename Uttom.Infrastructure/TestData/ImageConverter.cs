namespace Uttom.Infrastructure.TestData;

/// <summary>
///  only for testing purpose. This class is used to convert image files from project root to base64 strings.
/// </summary>
/// <remarks>TODO: Move to the appropriate project.</remarks>
public static class ImageConverter
{
    private static readonly string RelativePath = "Uttom.Infrastructure/TestData/Images";

    public static string ConvertToBase64(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("File name cannot be null or empty.", nameof(fileName));
        }

        var projectRoot = GetProjectRoot();
        var filePath = Path.Combine(projectRoot, RelativePath, fileName);

        try
        {
            var imageBytes = File.ReadAllBytes(filePath);
            return Convert.ToBase64String(imageBytes);
        }
        catch (FileNotFoundException)
        {
            throw new FileNotFoundException($"File '{fileName}' not found at '{filePath}'.");
        }
        catch (Exception ex)
        {
            throw new Exception($"An error occurred while reading the file: {ex.Message}", ex);
        }
    }

    private static string GetProjectRoot()
    {
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

        return Path.GetFullPath(Path.Combine(baseDirectory, "../../../.."));
    }
}