using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Uttom.Domain.Models;

namespace Uttom.Domain.Messages;

public class RegisteredMotorcycle : Entity
{
    [JsonConstructor]
    private RegisteredMotorcycle(string identifier, int year, string model, string plateNumber)
    {
        Identifier = identifier;
        Year = year;
        Model = model;
        PlateNumber = plateNumber;
    }

    [Required]
    public string Identifier { get; private set; }

    [Required]
    public int Year { get; private set; }

    [Required]
    public string Model { get; private set; }

    [Required]
    public string PlateNumber { get; private set; }

    public static RegisteredMotorcycle Create(string identifier, int year, string model, string plateNumber)
    {
        return new RegisteredMotorcycle(identifier, year, model, plateNumber);
    }
}