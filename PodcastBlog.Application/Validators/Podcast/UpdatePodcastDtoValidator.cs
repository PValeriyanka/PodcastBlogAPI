using FluentValidation;
using PodcastBlog.Application.ModelsDto.Podcast;

namespace PodcastBlog.Application.Validators.Podcast
{
    public class UpdatePodcastDtoValidator : AbstractValidator<UpdatePodcastDto>
    {
        public UpdatePodcastDtoValidator()
        {
            RuleFor(x => x.PodcastId)
                .GreaterThan(0);

            RuleFor(x => x.Title)
                .MaximumLength(100)
                .When(x => x.Title != null);
        }
    }
}
