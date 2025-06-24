using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using PodcastBlog.Domain.Models;


namespace PodcastBlog.Infrastructure
{
    public class PodcastBlogContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public PodcastBlogContext(DbContextOptions<PodcastBlogContext> options) : base(options) { }

        public DbSet<Comment> Comments { get; set; }
        public DbSet<Podcast> Podcasts { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<UserSubscription> UserSubscriptions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Настройка таблицы Post
            modelBuilder.Entity<Post>(entity =>
            {
                entity.HasKey(p => p.PostId);

                // Связь "один-ко-многим": Post -> Comment
                entity.HasMany(p => p.Comments)
                      .WithOne(c => c.Post)
                      .HasForeignKey(c => c.PostId)
                      .OnDelete(DeleteBehavior.NoAction);

                // Связь "один-к-одному": Post - Podcast 
                entity.HasOne(p => p.Podcast)
                      .WithOne()
                      .HasForeignKey<Post>(p => p.PodcastId)
                      .IsRequired(false);

                // Связь "многие-ко-многим": Tag <-> Post
                entity.HasMany(p => p.Tags)
                      .WithMany(t => t.Posts)
                      .UsingEntity<Dictionary<string, object>>(
                      "PostTag",
                          j => j.HasOne<Tag>().WithMany(),
                          j => j.HasOne<Post>().WithMany()
                      );
            });

            // Настройка таблицы Tag
            modelBuilder.Entity<Tag>(entity =>
            {
                entity.HasKey(t => t.TagId);
                entity.HasIndex(t => t.Name).IsUnique();
            });

            // Настройка таблицы Comment 
            modelBuilder.Entity<Comment>(entity =>
            {
                entity.HasKey(c => c.CommentId);

                // Связь "один-ко-многим": Comment -> Comment (дерево ответов)
                entity.HasMany(c => c.Replies)
                      .WithOne(c => c.Parent)
                      .HasForeignKey(c => c.ParentId)
                      .IsRequired(false);
            });

            // Настройка таблицы UserSubscription 
            modelBuilder.Entity<UserSubscription>(entity =>
            {
                entity.HasKey(us => new { us.SubscriberId, us.AuthorId });

                // Связь "один-ко-многим": User -> User (подписки)
                entity.HasOne(us => us.Subscriber)
                      .WithMany(u => u.Subscriptions)
                      .HasForeignKey(us => us.SubscriberId)
                      .OnDelete(DeleteBehavior.NoAction);

                // Связь "один-ко-многим": User -> User (подписчики)
                entity.HasOne(us => us.Author)
                      .WithMany(u => u.Followers)
                      .HasForeignKey(us => us.AuthorId)
                      .OnDelete(DeleteBehavior.NoAction);

            });

            // Настройка таблицы User 
            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(u => u.Role).HasConversion<string>();

                // Связь "один-ко-многим": User -> Post
                entity.HasMany(u => u.Posts)
                      .WithOne(p => p.Author)
                      .HasForeignKey(p => p.AuthorId);

                // Связь "один-ко-многим": User -> Notification
                entity.HasMany(u => u.Notifications)
                      .WithOne(n => n.User)
                      .HasForeignKey(p => p.UserId);

                // Связь "многие-ко-многим": Post <-> User 
                entity.HasMany(u => u.Liked)
                      .WithMany(p => p.Likes)
                      .UsingEntity<Dictionary<string, object>>(
                          "UserPostLikes",
                          j => j.HasOne<Post>().WithMany().OnDelete(DeleteBehavior.NoAction),
                          j => j.HasOne<User>().WithMany().OnDelete(DeleteBehavior.NoAction)
                      );

                // Связь "один-ко-многим": User -> Comment
                entity.HasMany(u => u.Comments)
                      .WithOne(c => c.User)
                      .HasForeignKey(c => c.UserId);
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
            var admin = new User
            {
                Id = 1,
                UserName = "admin",
                Name = "admin",
                Role = UserRole.Administrator,
                Email = "lera.pinchukova@gmail.com",
                PasswordHash = new PasswordHasher<User>().HashPassword(null, "admin")
            };
            var author = new User
            {
                Id = 2,
                UserName = "author_1",
                Name = "Author #1",
                Role = UserRole.Author,
                Email = "author_1@blog.com",
                PasswordHash = new PasswordHasher<User>().HashPassword(null, "author_1")
            };
            modelBuilder.Entity<User>().HasData(author, admin);

            // Создание Tags
            var tag1 = new Tag
            {
                TagId = 1,
                Name = "Tag1"
            };
            var tag2 = new Tag
            {
                TagId = 2,
                Name = "Tag2"
            };
            modelBuilder.Entity<Tag>().HasData(tag1, tag2);

            // Создание Podcasts
            var podcast1 = new Podcast
            {
                PodcastId = 1,
                Title = "Podcast #1",
                AudioFile = "/audio/fake1.mp3",
                Duration = 180,
                ListenCount = 0,
                Transcript = "Это фиктивная транскрипция."
            };

            var podcast2 = new Podcast
            {
                PodcastId = 2,
                Title = "Podcast #2",
                AudioFile = "/audio/fake2.wav",
                Duration = 240,
                ListenCount = 0,
                Transcript = "Тут могла бы быть ваша реклама."
            };
            modelBuilder.Entity<Podcast>().HasData(podcast1, podcast2);


            // Создание Posts
            var post1 = new Post
            {
                PostId = 1,
                Title = "Post #1",
                Content = "The first Post",
                AuthorId = author.Id,
                PublishedAt = new DateTime(new DateOnly(2024, 10, 1), new TimeOnly(13, 30)),
                PodcastId = podcast1.PodcastId,
                Status = PostStatus.Scheduled,
                Views = 0
            };
            var post2 = new Post
            {
                PostId = 2,
                Title = "Post #2",
                Content = "The second Post",
                AuthorId = author.Id,
                PublishedAt = new DateTime(new DateOnly(2024, 11, 5), new TimeOnly(14, 30)),
                PodcastId = podcast2.PodcastId,
                Status = PostStatus.Draft,
                Views = 0
            };
            var post3 = new Post
            {
                PostId = 3,
                Title = "Post #3",
                Content = "The third Post",
                AuthorId = author.Id,
                PublishedAt = new DateTime(new DateOnly(2025, 12, 11), new TimeOnly(17, 35)),
                PodcastId = null,
                Status = PostStatus.Scheduled,
                Views = 0
            };
            modelBuilder.Entity<Post>().HasData(post1, post2, post3);

            // Создание Comments
            var comment1 = new Comment
            {
                CommentId = 1,
                PostId = post1.PostId,
                UserId = author.Id,
                Content = "Comment #1",
                CreatedAt = new DateTime(new DateOnly(2024, 11, 12), new TimeOnly(10, 45)),
                Status = CommentStatus.Approved
            };
            var comment2 = new Comment
            {
                CommentId = 2,
                PostId = post1.PostId,
                UserId = admin.Id,
                Content = "Comment #2",
                CreatedAt = new DateTime(new DateOnly(2025, 1, 5), new TimeOnly(17, 05)),
                Status = CommentStatus.Approved
            };
            var comment3 = new Comment
            {
                CommentId = 3,
                PostId = post1.PostId,
                UserId = author.Id,
                ParentId = comment2.CommentId,
                Content = "Comment #3",
                CreatedAt = new DateTime(new DateOnly(2025, 1, 5), new TimeOnly(17, 05)),
                Status = CommentStatus.Approved
            };
            modelBuilder.Entity<Comment>().HasData(comment1, comment2, comment3);
        }
    }
}



