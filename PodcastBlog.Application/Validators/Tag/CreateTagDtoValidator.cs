using FluentValidation;
using PodcastBlog.Application.ModelsDto.Tag;

namespace PodcastBlog.Application.Validators.Tag
{
    public class CreateTagDtoValidator : AbstractValidator<CreateTagDto>
    {
        public CreateTagDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Данное поле является обязательным")
                .MaximumLength(50);
        }
    }
}
