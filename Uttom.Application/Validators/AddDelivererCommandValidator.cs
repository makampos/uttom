using Uttom.Application.Features.Commands;

namespace Uttom.Application.Validators;

using FluentValidation;

public class AddDelivererCommandValidator : AbstractValidator<AddDelivererCommand>
{
    public AddDelivererCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(command => command.Identifier)
            .NotEmpty().WithMessage("Identifier is required.")
            .Length(1, 50).WithMessage("Identifier must be between 1 and 50 characters.");

        RuleFor(command => command.Name)
            .NotEmpty().WithMessage("Name is required.")
            .Length(1, 50).WithMessage("Name must be between 1 and 50 characters.");

        RuleFor(command => command.BusinessTaxId)
            .NotEmpty().WithMessage("Business Tax ID is required.")
            .Matches(@"^\d{14}$").WithMessage("Business Tax ID must be exactly 14 digits.");

        RuleFor(command => command.DateOfBirth)
            .NotEmpty().WithMessage("Date of Birth is required.")
            .Must(BeAValidAge).WithMessage("The deliverer must be at least 18 years old.");

        RuleFor(command => command.DriverLicenseNumber)
            .NotEmpty().WithMessage("Driver License Number is required.")
            .Matches(@"^\d{9}$").WithMessage("Driver License Number must be exactly 9 digits and contain only numbers.");

        RuleFor(command => command.DriverLicenseType)
            .InclusiveBetween(1, 3).WithMessage("Driver License Type must be between 1 and 3.");
    }

    private bool BeAValidAge(DateTime dateOfBirth)
    {
        var age = DateTime.Today.Year - dateOfBirth.Year;
        if (dateOfBirth > DateTime.Today.AddYears(-age)) age--;
        return age >= 18;
    }

}