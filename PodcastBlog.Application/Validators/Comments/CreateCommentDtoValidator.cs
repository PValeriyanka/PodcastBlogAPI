using FluentValidation;
using PodcastBlog.Application.ModelsDto.Comment;

namespace PodcastBlog.Application.Validators.Comments
{
    public class CreateCommentDtoValidator : AbstractValidator<CreateCommentDto>
    {
        public CreateCommentDtoValidator()
        {
            RuleFor(x => x.Content)
                .NotEmpty()
                .WithMessage("Данное поле является обязательным")
                .MaximumLength(500);

            RuleFor(x => x.PostId)
                .GreaterThan(0);

            RuleFor(x => x.ParentId)
                .GreaterThan(0)
                .When(x => x.ParentId.HasValue);
        }
    }
}
