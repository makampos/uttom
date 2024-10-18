using System.ComponentModel.DataAnnotations;
using Uttom.Domain.Enum;

namespace Uttom.Domain.Models;

public class Deliverer : TrackableEntity
{
    private Deliverer(string identifier, string name, string businessTaxId, DateTime dateOfBirth, string driverLicenseNumber, DriverLicenseType driverLicenseType, string? driverLicenseImageId)
    {
        Identifier = identifier;
        Name = name;
        BusinessTaxId = businessTaxId;
        DateOfBirth = dateOfBirth;
        DriverLicenseNumber = driverLicenseNumber;
        DriverLicenseType = driverLicenseType;
        DriverLicenseImageId = driverLicenseImageId;
    }

    [Required]
    public string Identifier { get; private set; }

    [Required]
    public string Name { get; private set; }

    [Required]
    public string BusinessTaxId { get; private set; }

    [Required]
    public DateTime DateOfBirth { get; private set; }

    [Required]
    public string DriverLicenseNumber { get; private set; }

    [Required]
    public DriverLicenseType DriverLicenseType { get; private set; }

    public string? DriverLicenseImageId { get; private set; } = null;

    public static Deliverer Create(string identifier, string name, string businessTaxId, DateTime dateOfBirth, string driverLicenseNumber, DriverLicenseType driverLicenseType)
    {
        return new Deliverer(identifier, name, businessTaxId, dateOfBirth, driverLicenseNumber, driverLicenseType, null);
    }

    public void AddOrUpdateDriverLicenseImageId(string driverLicenseImageId)
    {
        DriverLicenseImageId = driverLicenseImageId;
    }
}