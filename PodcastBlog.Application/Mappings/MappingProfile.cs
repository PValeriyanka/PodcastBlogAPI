using AutoMapper;
using PodcastBlog.Application.ModelsDto.Comment;
using PodcastBlog.Application.ModelsDto.Notification;
using PodcastBlog.Application.ModelsDto.Podcast;
using PodcastBlog.Application.ModelsDto.Post;
using PodcastBlog.Application.ModelsDto.Tag;
using PodcastBlog.Application.ModelsDto.User;
using PodcastBlog.Domain.Models;

namespace PodcastBlog.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Comment, CommentDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName));
            CreateMap<CreateCommentDto, Comment>();

            CreateMap<Notification, NotificationDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.Name));

            CreateMap<Podcast, PodcastDto>();
            CreateMap<CreatePodcastDto, Podcast>();
            CreateMap<UpdatePodcastDto, Podcast>()
                .ForAllMembers(opt => opt.Condition((src, dest, val) => val != null));

            CreateMap<Post, PostDto>()
                .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.Author.Name))
                .ForMember(dest => dest.LikesCount, opt => opt.MapFrom(src => src.Likes.Count))
                .ForMember(dest => dest.Podcast, opt => opt.MapFrom(src => src.Podcast))
                .ForMember(dest => dest.Tags,
                    opt => opt.MapFrom(src =>
                        src.Tags != null
                            ? src.Tags.Select(t => $"#{t.Name}").ToList()
                            : new List<string>()));
            CreateMap<CreatePostDto, Post>()
                .ForMember(dest => dest.Tags, opt => opt.Ignore());
            CreateMap<UpdatePostDto, Post>()
                .ForMember(dest => dest.Tags, opt => opt.Ignore());

            CreateMap<Tag, TagDto>();
            CreateMap<CreateTagDto, Tag>();

            CreateMap<User, UserDto>()
                .ForMember(dest => dest.PostsCount, opt => opt.MapFrom(src => src.Posts.Count))
                .ForMember(dest => dest.FollowersCount, opt => opt.MapFrom(src => src.Followers.Count))
                .ForMember(dest => dest.SubscriptionsCount, opt => opt.MapFrom(src => src.Subscriptions.Count));
            CreateMap<UpdateUserDto, User>().ForAllMembers(opt => opt.Condition((src, _, val) => val != null));
        }
    }
}
