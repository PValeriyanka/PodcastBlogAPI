﻿using AutoMapper;
using Microsoft.Extensions.Logging;
using PodcastBlog.Application.Interfaces.Services;
using PodcastBlog.Application.Interfaces.Strategies;
using PodcastBlog.Application.ModelsDto.User;
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
        private readonly ILogger<UserService> _logger;

        public UserService(IUnitOfWork unitOfWork, INotificationService notificationService, IUserCleanupStrategy cleanup, IMapper mapper, ILogger<UserService> logger)
        {
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
            _cleanup = cleanup;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PagedList<UserDto>> GetAllUsersPagedAsync(Parameters parameters, CancellationToken cancellationToken)
        {
            try
            {
                var users = await _unitOfWork.Users.GetAllUsersPagedAsync(parameters, cancellationToken);

                var usersDto = _mapper.Map<IEnumerable<UserDto>>(users).ToList();

                _logger.LogInformation("Список пользователей успешно получен");
                return new PagedList<UserDto>(usersDto, users.MetaData.TotalCount, users.MetaData.CurrentPage, users.MetaData.PageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении списка пользователей");
                throw;
            }
        }

        public async Task<UserDto> GetUserByIdAsync(int id, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(id, cancellationToken);

                if (user is null)
                {
                    _logger.LogWarning("Получение. Пользователь не найден");
                    return null;
                }

                var userDto = _mapper.Map<UserDto>(user);

                _logger.LogInformation("Пользователь успешно получен");
                return userDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении пользователя");
                throw;
            }
        }

        public async Task UpdateUserAsync(UpdateUserDto updateUserDto, ClaimsPrincipal userPrincipal, CancellationToken cancellationToken)
        {
            int.TryParse(userPrincipal.FindFirstValue(ClaimTypes.NameIdentifier), out int userId);

            var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);

            if (user is null)
            {
                _logger.LogWarning("Обновление. Пользователь не найден");
                return;
            }

            _mapper.Map(updateUserDto, user);

            if (userId != user.Id)
            {
                _logger.LogWarning("Профиль изменить может только создатель");
                return;
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Профиль пользователя успешно обновлен");
        }

        public async Task UpdateUserRoleAsync(UpdateUserRoleDto updateUserRoleDto, CancellationToken cancellationToken)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(updateUserRoleDto.UserId, cancellationToken);

            if (user is null)
            {
                _logger.LogWarning("Изменение роли. Пользователь не найден");
                return;
            }

            user.Role = updateUserRoleDto.Role;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Роль пользователя успешно обновлена");
        }

        public async Task DeleteUserAsync(int id, ClaimsPrincipal userPrincipal, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(id, cancellationToken);

                if (user is null)
                {
                    _logger.LogWarning("Удаление. Пользователь не найден");
                    return;
                }

                int.TryParse(userPrincipal.FindFirstValue(ClaimTypes.NameIdentifier), out int userId);

                if (userId != user.Id && user.Role != UserRole.Administrator)
                {
                    _logger.LogWarning("Профиль удалить может только создатель или администатор");
                    return;
                }

                await _cleanup.CleanupAsync(user, userPrincipal, cancellationToken);

                await _unitOfWork.Users.DeleteAsync(user, cancellationToken);

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Пользователь успешно удален");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении пользователя");
                throw;
            }
        }

        public async Task SubscriptionAsync(int authorId, ClaimsPrincipal userPrincipal, CancellationToken cancellationToken)
        {
            try
            {
                int.TryParse(userPrincipal.FindFirstValue(ClaimTypes.NameIdentifier), out int subscriberId);

                if (subscriberId == authorId)
                {
                    _logger.LogWarning("Попытка подписки на самого себя");
                    return;
                }

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

                    _logger.LogInformation("Отписка произведена успешно");
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
                    _logger.LogInformation("Подписка произведена успешно");
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при попытке подписки/отписки");
                throw;
            }
        }

        public async Task PostLikeAsync(int postId, ClaimsPrincipal userPrincipal, CancellationToken cancellationToken)
        {
            try
            {
                int.TryParse(userPrincipal.FindFirstValue(ClaimTypes.NameIdentifier), out int userId);

                var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
                var post = await _unitOfWork.Posts.GetByIdAsync(postId, cancellationToken);

                var alreadyLiked = post.Likes.Any(u => u.Id == userId);

                if (alreadyLiked)
                {
                    post.Likes.Remove(user);
                    user.Liked.Remove(post);

                    _logger.LogInformation("Лайк успешно удален");
                }
                else
                {
                    post.Likes.Add(user);
                    user.Liked.Add(post);

                    await _notificationService.NewLikeNotificationAsync(userId, post, cancellationToken);

                    _logger.LogInformation("Лайк успешно добавлен");
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при добавлении/удалении лайка");
                throw;
            }
        }
    }
}