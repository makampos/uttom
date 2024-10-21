using System.Text;

namespace Uttom.UnitTests.TestHelpers;

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

    protected string GeneratePlateNumber()
    {
        var rnd = new Random();
        var builder = new StringBuilder();
        for (var i = 0; i < 3; i++)
        {
            builder.Append((char)rnd.Next('A', 'Z' + 1));
        }

        builder.Append('-');

        for (var i = 0; i < 4; i++)
        {
            builder.Append(rnd.Next(10));
        }

        return builder.ToString(); // e.g. "ABC-1234"
    }

    protected enum DocumentType
    {
        BusinessTaxId,
        DriverLicenseNumber,
    }
}