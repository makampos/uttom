using FluentValidation.TestHelper;
using Uttom.Application.Features.Commands;
using Uttom.Application.Validators;

namespace Uttom.UnitTests.Validators
{
    public class AddDelivererCommandValidatorTests
    {
        private readonly AddDelivererCommandValidator _validator;

        public AddDelivererCommandValidatorTests()
        {
            _validator = new AddDelivererCommandValidator();
        }

        [Fact]
        public void Should_Have_Error_When_Identifier_Is_Empty()
        {
            // Arrange
            var command = new AddDelivererCommand(string.Empty, "Matheus", "68553667000155", DateTime.Now.AddYears(-20), "123456789", 1, null);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Identifier)
                .WithErrorMessage("Identifier is required.");
        }

        [Fact]
        public void Should_Have_Error_When_Name_Is_Empty()
        {
            // Arrange
            var command = new AddDelivererCommand("ID123", string.Empty, "68553667000155", DateTime.Now.AddYears(-20), "123456789", 1, null);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Name)
                .WithErrorMessage("Name is required.");
        }

        [Fact]
        public void Should_Have_Error_When_BusinessTaxId_Is_Empty()
        {
            // Arrange
            var command = new AddDelivererCommand("ID123", "Matheus", string.Empty, DateTime.Now.AddYears(-20), "123456789", 1, null);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.BusinessTaxId)
                .WithErrorMessage("Business Tax ID is required.");
        }

        [Fact]
        public void Should_Have_Error_When_BusinessTaxId_Is_Not_14_Digits()
        {
            // Arrange
            var command = new AddDelivererCommand("ID123", "Matheus", "1234567890123", DateTime.Now.AddYears(-20), "123456789", 1, null);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.BusinessTaxId)
                .WithErrorMessage("Business Tax ID must be exactly 14 digits.");
        }

        [Fact]
        public void Should_Have_Error_When_DateOfBirth_Is_Empty()
        {
            // Arrange
            var command = new AddDelivererCommand("ID123", "Matheus", "12345678901234", default, "123456789", 1, null);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.DateOfBirth)
                .WithErrorMessage("Date of Birth is required.");
        }

        [Fact]
        public void Should_Have_Error_When_DateOfBirth_Is_Under_18()
        {
            // Arrange
            var command = new AddDelivererCommand("ID123", "Matheus", "12345678901234", DateTime.Now.AddYears(-10), "123456789", 1, null);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.DateOfBirth)
                .WithErrorMessage("The deliverer must be at least 18 years old.");
        }

        [Fact]
        public void Should_Have_Error_When_DriverLicenseNumber_Is_Empty()
        {
            // Arrange
            var command = new AddDelivererCommand("ID123", "Matheus", "12345678901234", DateTime.Now.AddYears(-20), string.Empty, 1, null);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.DriverLicenseNumber)
                .WithErrorMessage("Driver License Number is required.");
        }

        [Fact]
        public void Should_Have_Error_When_DriverLicenseNumber_Is_Not_9_Digits()
        {
            // Arrange
            var command = new AddDelivererCommand("ID123", "Matheus", "12345678901234", DateTime.Now.AddYears(-20), "12345", 1, null);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.DriverLicenseNumber)
                .WithErrorMessage("Driver License Number must be exactly 9 digits and contain only numbers.");
        }

        [Fact]
        public void Should_Have_Error_When_DriverLicenseType_Is_Out_Of_Range()
        {
            // Arrange
            var command = new AddDelivererCommand("ID123", "Matheus", "12345678901234", DateTime.Now.AddYears(-20), "123456789", 4, null);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.DriverLicenseType)
                .WithErrorMessage("Driver License Type must be between 1 and 3.");
        }
    }
}