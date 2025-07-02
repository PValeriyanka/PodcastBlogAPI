using FluentValidation;
using PodcastBlog.Application.ModelsDto.Authentication;

namespace PodcastBlog.Application.Validators.Auth
{
    public class RegistrationDtoValidator : AbstractValidator<RegistrationDto>
    {
        public RegistrationDtoValidator()
        {
            RuleFor(x => x.UserName)
                .NotEmpty()
                .WithMessage("Данное поле является обязательным")
                .MaximumLength(50);

            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Данное поле является обязательным")
                .MaximumLength(100);

            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("Данное поле является обязательным")
                .EmailAddress();

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage("Данное поле является обязательным")
                .MinimumLength(6);
        }
    }
}
