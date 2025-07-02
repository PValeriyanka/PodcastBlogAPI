using FluentValidation;
using PodcastBlog.Application.ModelsDto.Podcast;

namespace PodcastBlog.Application.Validators.Podcast
{
    public class CreatePodcastDtoValidator : AbstractValidator<CreatePodcastDto>
    {
        public CreatePodcastDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty()
                .WithMessage("Данное поле является обязательным")
                .MaximumLength(100);

            RuleFor(x => x.AudioUpload)
                .NotNull()
                .WithMessage("Данное поле является обязательным");

            RuleFor(x => x.CoverImageUpload)
                .NotNull()
                .When(x => x.CoverImageUpload != null);
        }
    }
}
