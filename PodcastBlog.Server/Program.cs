using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PodcastBlog.Application.Interfaces.Services;
using PodcastBlog.Application.Interfaces.Strategies;
using PodcastBlog.Application.Mappings;
using PodcastBlog.Application.Services;
using PodcastBlog.Application.Strategies;
using PodcastBlog.Domain.Interfaces;
using PodcastBlog.Domain.Interfaces.Repositories;
using PodcastBlog.Domain.Models;
using PodcastBlog.Infrastructure;
using PodcastBlog.Infrastructure.Repositories;

namespace PodcastBlog.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            string? connectionString = builder.Configuration.GetConnectionString("RemoteConnection");
            builder.Services.AddDbContext<PodcastBlogContext>(options => options.UseSqlServer(connectionString));
            
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            builder.Services.AddScoped<ICommentRepository, CommentRepository>();
            builder.Services.AddScoped<ICommentService, CommentService>();
            builder.Services.AddScoped<ICommentCleanupStrategy, CommentCleanupStrategy>();

            builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddScoped<INotificationCleanupStrategy, NotificationCleanupStrategy>();

            builder.Services.AddScoped<IEmailService, EmailService>();

            builder.Services.AddScoped<IPodcastRepository, PodcastRepository>();
            builder.Services.AddScoped<IPodcastService, PodcastService>();
            builder.Services.AddScoped<IPodcastCleanupStrategy, PodcastCleanupStrategy>();

            builder.Services.AddScoped<IMediaService, MediaService>();

            builder.Services.AddScoped<IPostRepository, PostRepository>();
            builder.Services.AddScoped<IPostService, PostService>();
            builder.Services.AddScoped<IPostCleanupStrategy, PostCleanupStrategy>();

            builder.Services.AddScoped<ITagRepository, TagRepository>();
            builder.Services.AddScoped<ITagService, TagService>();
            builder.Services.AddScoped<ITagCleanupStrategy, TagCleanupStrategy>();

            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IUserCleanupStrategy, UserCleanupStrategy>();

            builder.Services.AddIdentity<User, IdentityRole<int>>()
                    .AddEntityFrameworkStores<PodcastBlogContext>()
                    .AddDefaultTokenProviders();

            builder.Services.AddSwaggerGen();

            builder.Services.AddControllers();

            builder.Services.AddAutoMapper(typeof(MappingProfile));

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseStaticFiles();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Post}/{action=Index}/{id?}");

            app.MapControllers();

            app.Run();
        }
    }
}
