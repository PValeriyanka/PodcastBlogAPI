using FluentValidation;
using PodcastBlog.Application.ModelsDto.User;

namespace PodcastBlog.Application.Validators.User
{
    public class UpdateUserRoleDtoValidator : AbstractValidator<UpdateUserRoleDto>
    {
        public UpdateUserRoleDtoValidator()
        {
            RuleFor(x => x.UserId)
                .GreaterThan(0);

            RuleFor(x => x.Role)
                .IsInEnum();
        }
    }
}
