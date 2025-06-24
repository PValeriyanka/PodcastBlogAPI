using AutoMapper;
using PodcastBlog.Domain.Models;
using PodcastBlog.Application.ModelsDto;

namespace PodcastBlog.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Comment, CommentDto>()
                .ReverseMap();

            CreateMap<Notification, NotificationDto>()
                .ReverseMap();

            CreateMap<Podcast, PodcastDto>()
                .ReverseMap();

            CreateMap<Post, PostDto>()
                .ForMember(dest => dest.Tags,
                    opt => opt.MapFrom(src =>
                        string.Join(" ", src.Tags.Select(t => $"#{t.Name}"))))
                .ReverseMap()
                .ForMember(dest => dest.Tags, opt => opt.Ignore());

            CreateMap<Tag, TagDto>()
                .ReverseMap();

            CreateMap<User, UserDto>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
                .ReverseMap()
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore());
        }
    }
}
