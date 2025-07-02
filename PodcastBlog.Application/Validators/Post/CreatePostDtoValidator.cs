using FluentValidation;
using PodcastBlog.Application.ModelsDto.Post;

namespace PodcastBlog.Application.Validators.Post
{
    public class CreatePostDtoValidator : AbstractValidator<CreatePostDto>
    {
        public CreatePostDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty()
                .WithMessage("Данное поле является обязательным")
                .MaximumLength(100);

            RuleFor(x => x.Content)
                .NotEmpty()
                .WithMessage("Данное поле является обязательным");

            RuleFor(x => x.PublishedAt)
                .LessThanOrEqualTo(DateTime.Now)
                .When(x => x.PublishedAt.HasValue);

            RuleFor(x => x.Tags)
                .MaximumLength(200)
                .When(x => x.Tags != null);

            RuleFor(x => x.PodcastId)
                .GreaterThan(0)
                .When(x => x.PodcastId.HasValue);
        }
    }
}
