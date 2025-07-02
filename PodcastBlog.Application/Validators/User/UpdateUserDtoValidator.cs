using FluentValidation;
using PodcastBlog.Application.ModelsDto.User;

namespace PodcastBlog.Application.Validators.User
{
    public class UpdateUserDtoValidator : AbstractValidator<UpdateUserDto>
    {
        public UpdateUserDtoValidator()
        {
            RuleFor(x => x.Name)
                .MaximumLength(100)
                .When(x => x.Name != null);
        }
    }
}
