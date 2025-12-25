using FluentValidation;
using HanaHotel.WebUI.DTOs.GuestDTO;

namespace HanaHotel.WebUI.ValidationRules.AdminGuestValidationRules
{
    public class CreateGuestValidator : AbstractValidator<CreateGuestDTO>
    {
        public CreateGuestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name cannot be empty")
                .Length(2, 50).WithMessage("Name must be between 2 and 50 characters");

            RuleFor(x => x.Surname)
                .NotEmpty().WithMessage("Surname cannot be empty")
                .Length(2, 50).WithMessage("Surname must be between 2 and 50 characters");

            RuleFor(x => x.City)
                .NotEmpty().WithMessage("City cannot be empty");
        }
    }
}

