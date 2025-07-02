using FluentValidation;
using PodcastBlog.Application.ModelsDto.Post;

namespace PodcastBlog.Application.Validators.Post
{
    public class UpdatePostDtoValidator : AbstractValidator<UpdatePostDto>
    {
        public UpdatePostDtoValidator()
        {
            RuleFor(x => x.PostId)
                .GreaterThan(0);

            RuleFor(x => x.Title)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.Tags)
                .MaximumLength(200)
                .When(x => x.Tags != null);

            RuleFor(x => x.PublishedAt)
                .LessThanOrEqualTo(DateTime.Now)
                .When(x => x.PublishedAt.HasValue);

            RuleFor(x => x.PodcastId)
                .GreaterThan(0)
                .When(x => x.PodcastId.HasValue);
        }
    }
}
