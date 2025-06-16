using PodcastBlog.Application.ModelsDTO;

namespace PodcastBlog.Application.IServices
{
    public interface IUserService
    {
        Task<UserDTO> GetUserById(int id, CancellationToken cancellationToken);
        Task CreateUser(UserDTO userDTO, CancellationToken cancellationToken);
        Task UpdateUser(UserDTO userDTO, CancellationToken cancellationToken);
        Task DeleteUser(int id, CancellationToken cancellationToken);
    }
}
