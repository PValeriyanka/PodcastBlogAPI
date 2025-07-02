using FluentValidation;
using PodcastBlog.Application.ModelsDto.Authentication;

namespace PodcastBlog.Application.Validators.Auth
{
    public class LoginDtoValidator : AbstractValidator<LoginDto>
    {
        public LoginDtoValidator()
        {
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
