using System.Text;

namespace Uttom.IntegrationTests.Helpers;

public class TestHelper
{
    protected string GenerateDocument(DocumentType type)
    {
        var rnd = new Random();

        var builder = new StringBuilder();

        var digits = type switch
        {
            DocumentType.BusinessTaxId => 14,
            DocumentType.DriverLicenseNumber => 9,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        for (var i = 0; i < digits; i++)
        {
            builder.Append(rnd.Next(10));
        }

        return builder.ToString();
    }

    protected enum DocumentType
    {
        BusinessTaxId,
        DriverLicenseNumber,
    }
}

