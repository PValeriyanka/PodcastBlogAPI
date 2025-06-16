using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using PodcastBlog.Domain.Models;


namespace PodcastBlog.Infrastructure
{
    public class PodcastBlogContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public PodcastBlogContext(DbContextOptions<PodcastBlogContext> options) : base(options)
        {
        }

        public DbSet<Comment> Comments { get; set; }
        public DbSet<Podcast> Podcasts { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<UserSubscription> UserSubscriptions { get; set; } 

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Настройка таблицы Post
            modelBuilder.Entity<Post>(entity =>
            {
                entity.HasKey(p => p.PostId);

                // Связь "один-ко-многим": User -> Post
                entity.HasOne(p => p.Author)
                      .WithMany(u => u.Posts)
                      .HasForeignKey(p => p.AuthorId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Связь "один-к-одному": Post -> Podcast 
                entity.HasOne(p => p.Podcast)
                      .WithMany()
                      .HasForeignKey(p => p.PodcastId)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Настройка таблицы Comment 
            modelBuilder.Entity<Comment>(entity =>
            {
                entity.HasKey(c => c.CommentId);

                // Связь "один-ко-многим": Post -> Comment
                entity.HasOne(c => c.Post)
                      .WithMany(p => p.Comments)
                      .HasForeignKey(c => c.PostId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Связь "один-ко-многим": User -> Comment
                entity.HasOne(c => c.User)
                      .WithMany(u => u.Comments)
                      .HasForeignKey(c => c.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Связь "один-ко-многим": Comment -> Comment (дерево ответов)
                entity.HasOne(c => c.Parent)
                      .WithMany(c => c.Replies)
                      .HasForeignKey(c => c.ParentId)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Настройка таблицы Tag 
            modelBuilder.Entity<Tag>(entity =>
            {
                entity.HasKey(t => t.TagId);
                entity.HasIndex(t => t.Name).IsUnique();
            });

            // Настройка таблицы UserSubscription 
            modelBuilder.Entity<UserSubscription>(entity =>
            {
                entity.HasKey(us => new { us.SubscriberId, us.AuthorId });

                // Связь "один-ко-многим": User -> User (подписки)
                entity.HasOne(us => us.Subscriber)
                      .WithMany(u => u.Subscriptions)
                      .HasForeignKey(us => us.SubscriberId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Связь "один-ко-многим": User -> User (подписчики)
                entity.HasOne(us => us.Author)
                      .WithMany(u => u.Followers)
                      .HasForeignKey(us => us.AuthorId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Настройка таблицы User 
            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(u => u.Role).HasConversion<string>();
            });

            CreateData(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
        }

        private void CreateData(ModelBuilder modelBuilder)
        {
            // Создание Users
            var admin = new User { 
                Id = 1, 
                UserName = "admin", 
                Name = "admin", 
                Role = UserRole.Administrator, 
                Email = "admin@blog.com", 
                PasswordHash = new PasswordHasher<User>().HashPassword(null, "admin") 
            };
            var author = new User { 
                Id = 2, 
                UserName = "author_1", 
                Name = "Author #1", 
                Role = UserRole.Author, 
                Email = "author_1@blog.com", 
                PasswordHash = new PasswordHasher<User>().HashPassword(null, "author_1") 
            };
            modelBuilder.Entity<User>().HasData(author, admin);

            // Создание Tags
            var tag1 = new Tag { 
                TagId = 1, 
                Name = "Tag1"
            };
            var tag2 = new Tag { 
                TagId = 2, 
                Name = "Tag2" 
            };
            modelBuilder.Entity<Tag>().HasData(tag1, tag2);

            // Создание Podcasts
            var podcast1 = new Podcast { 
                PodcastId = 1, 
                Title = "Podcast #1", 
                AudioFile = "podcast_1.mp3", 
                Duration = 30, 
                EpisodeNumber = 1
            };
            var podcast2 = new Podcast { 
                PodcastId = 2, 
                Title = "Podcast #2", 
                AudioFile = "podcast_2.mp3", 
                Duration = 45, 
                EpisodeNumber = 2
            };
            modelBuilder.Entity<Podcast>().HasData(podcast1, podcast2);

            // Создание Posts
            var post1 = new Post { 
                PostId = 1, 
                Title = "Post #1", 
                Content = "The first Post", 
                AuthorId = author.Id, 
                PublishedAt = new DateTime(new DateOnly(2024, 10, 1), new TimeOnly(13, 30)),
                PodcastId = podcast1.PodcastId, 
                Status = PostStatus.Published, 
                Views = 0, 
                Likes = 0 
            };
            var post2 = new Post { 
                PostId = 2, 
                Title = "Post #2", 
                Content = "The second Post", 
                AuthorId = author.Id, 
                PublishedAt = new DateTime(new DateOnly(2024, 12, 11), new TimeOnly(14, 30)),
                PodcastId = podcast2.PodcastId, 
                Status = PostStatus.Published, 
                Views = 0, 
                Likes = 0 
            };
            modelBuilder.Entity<Post>().HasData(post1, post2);

            // Создание Comments
            var comment1 = new Comment { 
                CommentId = 1, 
                PostId = post1.PostId, 
                UserId = author.Id, 
                Content = "Comment #1", 
                CreatedAt = new DateTime(new DateOnly(2024, 11, 12), new TimeOnly(10, 45)),
                Status = CommentStatus.Approved 
            };
            var comment2 = new Comment { 
                CommentId = 2, 
                PostId = post1.PostId,  
                UserId = admin.Id, 
                Content = "Comment #2", 
                CreatedAt = new DateTime(new DateOnly(2025, 1, 5), new TimeOnly(17, 05)),
                Status = CommentStatus.Approved 
            };
            modelBuilder.Entity<Comment>().HasData(comment1, comment2);
        }
    }
}



