using Uttom.Application.Features.Commands;
using Uttom.Application.Validators;
using FluentValidation.TestHelper;
using Uttom.UnitTests.TestHelpers;

namespace Uttom.UnitTests.Validators;

public class AddMotorcycleCommandValidatorTests : TestHelper
{
    private readonly AddMotorcycleCommandValidator _validator;

    public AddMotorcycleCommandValidatorTests()
    {
        _validator = new AddMotorcycleCommandValidator();
    }

    [Fact]
    public void Should_Have_Error_When_Identifier_Is_Empty()
    {
        // Arrange
        var command = new AddMotorcycleCommand(string.Empty, 2022, "Yamaha", GeneratePlateNumber());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Identifier)
              .WithErrorMessage("Identifier is required.");
    }

    [Fact]
    public void Should_Have_Error_When_Identifier_Is_Too_Long()
    {
        // Arrange
        var command = new AddMotorcycleCommand(new string('A', 51), 2022, "Yamaha", GeneratePlateNumber());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Identifier)
              .WithErrorMessage("Identifier must be between 1 and 50 characters.");
    }

    [Fact]
    public void Should_Have_Error_When_Year_Is_Out_Of_Range()
    {
        // Arrange
        var command = new AddMotorcycleCommand("ID123", 1899, "Yamaha", GeneratePlateNumber());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Year)
              .WithErrorMessage($"Year must be between 1900 and {DateTime.Now.AddYears(1).Year}.");
    }

    [Fact]
    public void Should_Have_Error_When_Model_Is_Empty()
    {
        // Arrange
        var command = new AddMotorcycleCommand("ID123", 2022, string.Empty, GeneratePlateNumber());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Model)
              .WithErrorMessage("Model is required.");
    }

    [Fact]
    public void Should_Have_Error_When_Model_Is_Too_Long()
    {
        // Arrange
        var command = new AddMotorcycleCommand("ID123", 2022, new string('A', 51), GeneratePlateNumber());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Model)
              .WithErrorMessage("Model must be between 1 and 50 characters.");
    }

    [Fact]
    public void Should_Have_Error_When_PlateNumber_Is_Empty()
    {
        // Arrange
        var command = new AddMotorcycleCommand("ID123", 2022, "Yamaha", string.Empty);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.PlateNumber)
              .WithErrorMessage("Plate Number is required.");
    }

    [Fact]
    public void Should_Have_Error_When_PlateNumber_Is_Invalid()
    {
        // Arrange
        var command = new AddMotorcycleCommand("ID123", 2022, "Yamaha", "abc-123"); // Lowercase letters are invalid

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.PlateNumber)
              .WithErrorMessage("Plate Number must contain only uppercase letters, digits, or hyphens.");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Command_Is_Valid()
    {
        // Arrange
        var command = new AddMotorcycleCommand("ID123", 2022, "Yamaha", GeneratePlateNumber());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}