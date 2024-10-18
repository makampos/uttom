using System.ComponentModel.DataAnnotations;

namespace Uttom.Domain.Models;

public class Motorcycle : TrackableEntity
{
    private Motorcycle(string identifier, int year, string model, string plateNumber)
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

    public static Motorcycle Create(string identifier, int year, string model, string plateNumber)
    {
        return new Motorcycle(identifier, year, model, plateNumber);
    }

    public void Update(string plateNumber)
    {
        PlateNumber = plateNumber;
    }
}