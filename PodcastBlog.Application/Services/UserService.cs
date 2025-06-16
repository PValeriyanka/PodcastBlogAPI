using AutoMapper;
using PodcastBlog.Application.IServices;
using PodcastBlog.Application.ModelsDTO;
using PodcastBlog.Domain.IRepositories;
using PodcastBlog.Domain.Models;

namespace PodcastBlog.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UserService(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<UserDTO> GetUserById(int id, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetUserById(id, cancellationToken);
            var userDTO = _mapper.Map<UserDTO>(user);

            return userDTO;
        }

        public async Task CreateUser(UserDTO userDTO, CancellationToken cancellationToken)
        {
            var user = _mapper.Map<User>(userDTO);

            await _userRepository.CreateUser(user, cancellationToken);
        }

        public async Task UpdateUser(UserDTO userDTO, CancellationToken cancellationToken)
        {
            var user = _mapper.Map<User>(userDTO);

            await _userRepository.UpdateUser(user, cancellationToken);
        }

        public async Task DeleteUser(int id, CancellationToken cancellationToken)
        {
            await _userRepository.DeleteUser(id, cancellationToken);
        }
    }
}
