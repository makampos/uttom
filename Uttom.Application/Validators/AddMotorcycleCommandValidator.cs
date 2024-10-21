using Uttom.Application.Features.Commands;

namespace Uttom.Application.Validators;

using FluentValidation;

public class AddMotorcycleCommandValidator : AbstractValidator<AddMotorcycleCommand>
{
    public AddMotorcycleCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(command => command.Identifier)
            .NotEmpty().WithMessage("Identifier is required.")
            .Length(1, 50).WithMessage("Identifier must be between 1 and 50 characters.");

        RuleFor(command => command.Year)
            .InclusiveBetween(1900, DateTime.Now.Year).WithMessage($"Year must be between 1900 and {DateTime.Now.AddYears(1).Year}.");

        RuleFor(command => command.Model)
            .NotEmpty().WithMessage("Model is required.")
            .Length(1, 50).WithMessage("Model must be between 1 and 50 characters.");

        RuleFor(command => command.PlateNumber)
            .NotEmpty().WithMessage("Plate Number is required.")
            .Matches(@"^[A-Z0-9-]+$").WithMessage("Plate Number must contain only uppercase letters, digits, or hyphens.");
    }
}