using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Taskedin.Service.DTOs;

namespace Taskedin.Service.Validators;

// Shape and length rules only; uniqueness and existence checks live in the service layer.
public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .Length(2, 100).WithMessage("First name must be between 2 and 100 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .Length(2, 100).WithMessage("Last name must be between 2 and 100 characters.")
            .Must((request, lastName) =>
                $"{request.FirstName?.Trim()} {lastName?.Trim()}".Trim().Length
                    <= FullNameMaxLength)
            .WithMessage("Full name (first and last name combined) must not exceed 200 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email is not a valid email address.")
            .MaximumLength(256).WithMessage("Email must not exceed 256 characters.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
            .MaximumLength(100).WithMessage("Password must not exceed 100 characters.");

        RuleFor(x => x.DepartmentId)
            .GreaterThan(0).WithMessage("DepartmentId must be greater than zero.");
    }

    private const int FullNameMaxLength = 200;
}
