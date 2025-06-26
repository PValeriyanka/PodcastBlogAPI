using AutoMapper;
using PodcastBlog.Application.Interfaces.Services;
using PodcastBlog.Application.Interfaces.Strategies;
using PodcastBlog.Application.ModelsDto;
using PodcastBlog.Domain.Interfaces;
using PodcastBlog.Domain.Models;
using PodcastBlog.Domain.Parameters;
using System.Security.Claims;

namespace PodcastBlog.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;
        private readonly IUserCleanupStrategy _cleanup;
        private readonly IMapper _mapper;

        public UserService(IUnitOfWork unitOfWork, INotificationService notificationService, IUserCleanupStrategy cleanup, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
            _cleanup = cleanup;
            _mapper = mapper;
        }

        public async Task<PagedList<UserDto>> GetAllUsersPagedAsync(Parameters parameters, CancellationToken cancellationToken)
        {
            var users = await _unitOfWork.Users.GetAllUsersPagedAsync(parameters, cancellationToken);

            var usersDto = _mapper.Map<IEnumerable<UserDto>>(users).ToList();

            return new PagedList<UserDto>(usersDto, users.MetaData.TotalCount, users.MetaData.CurrentPage, users.MetaData.PageSize);
        }

        public async Task<UserDto> GetUserByIdAsync(int id, CancellationToken cancellationToken)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id, cancellationToken);

            var userDto = _mapper.Map<UserDto>(user);

            return userDto;
        }

        public async Task CreateUserAsync(UserDto userDto, CancellationToken cancellationToken)
        {
            var user = _mapper.Map<User>(userDto);

            await _unitOfWork.Users.CreateAsync(user, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateUserAsync(UserDto userDto, CancellationToken cancellationToken)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userDto.UserId, cancellationToken);

            _mapper.Map(userDto, user);

            await _unitOfWork.Users.UpdateAsync(user, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteUserAsync(int id, CancellationToken cancellationToken)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id, cancellationToken);

            await _cleanup.CleanupAsync(user, cancellationToken);

            await _unitOfWork.Users.DeleteAsync(user, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task SubscriptionAsync(ClaimsPrincipal userPrincipal, int authorId, CancellationToken cancellationToken)
        {
            int.TryParse(userPrincipal.FindFirstValue(ClaimTypes.NameIdentifier), out int subscriberId);

            if (subscriberId != authorId)
            {
                var subscriber = await _unitOfWork.Users.GetByIdAsync(subscriberId, cancellationToken);

                var author = await _unitOfWork.Users.GetByIdAsync(authorId, cancellationToken);

                var subscription = subscriber.Subscriptions.FirstOrDefault(s => s.AuthorId == authorId);

                if (subscription is not null)
                {
                    subscriber.Subscriptions.Remove(subscription);
                    var reverse = author.Followers.FirstOrDefault(s => s.SubscriberId == subscriberId);

                    if (reverse is not null)
                    {
                        author.Followers.Remove(reverse);
                    }
                }
                else
                {
                    var newSubscription = new UserSubscription
                    {
                        SubscriberId = subscriberId,
                        AuthorId = authorId
                    };

                    subscriber.Subscriptions.Add(newSubscription);
                    author.Followers.Add(newSubscription);

                    await _notificationService.NewSubscriberNotificationAsync(subscriberId, authorId, cancellationToken);
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task PostLikeAsync(ClaimsPrincipal userPrincipal, int postId, CancellationToken cancellationToken)
        {
            int.TryParse(userPrincipal.FindFirstValue(ClaimTypes.NameIdentifier), out int userId);

            var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
            var post = await _unitOfWork.Posts.GetByIdAsync(postId, cancellationToken);

            var alreadyLiked = post.Likes.Any(u => u.Id == userId);

            if (alreadyLiked)
            {
                post.Likes.Remove(user);
                user.Liked.Remove(post);
            }
            else
            {
                post.Likes.Add(user);
                user.Liked.Add(post);

                await _notificationService.NewLikeNotificationAsync(userId, post, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}