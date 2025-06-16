using AutoMapper;
using PodcastBlog.Domain.Models;
using PodcastBlog.Application.ModelsDTO;

namespace PodcastBlog.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Comment, CommentDTO>()
                .ReverseMap();

            CreateMap<Podcast, PodcastDTO>()
                .ReverseMap();

            CreateMap<Post, PostDTO>()
                .ReverseMap();

            CreateMap<Tag, TagDTO>()
                .ReverseMap();

            CreateMap<User, UserDTO>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
                .ReverseMap()
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore());
        }
    }
}
