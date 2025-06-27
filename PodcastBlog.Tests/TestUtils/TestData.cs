using PodcastBlog.Domain.Models;

namespace PodcastBlog.Tests.TestUtils
{
    public static class TestData
    {
        public static User User => new User
        {
            Id = 1,
            UserName = "test",
            Name = "Test",
            Email = "test@blog.com",
            EmailNotify = true,
            Role = UserRole.Author
        };

        public static User Another => new User
        {
            Id = 2,
            UserName = "another",
            Name = "Another",
            Email = "another@blog.com",
            EmailNotify = true,
            Role = UserRole.Author
        };

        public static User Admin => new User
        {
            Id = 3,
            UserName = "admin",
            Name = "Admin",
            Email = "admin@blog.com",
            EmailNotify = false,
            Role = UserRole.Administrator
        };

        public static Post Post => new Post
        {
            PostId = 1,
            Title = "Test post 1",
            Content = "This is test post #1",
            AuthorId = User.Id,
            Author = User,
            PublishedAt = DateTime.UtcNow,
            Status = PostStatus.Published,
            Tags = [TagTest],
            Likes = [Admin]
        };

        public static Post DraftPost => new Post
        {
            PostId = 2,
            Title = "Test post 2",
            Content = "This is test post #2",
            AuthorId = User.Id,
            Author = User,
            PublishedAt = DateTime.UtcNow,
            Status = PostStatus.Draft,
            Tags = [TagTest],
            Likes = [Admin]
        };

        public static Post SheduledPost => new Post
        {
            PostId = 3,
            Title = "Test post 3",
            Content = "This is test post #3",
            AuthorId = User.Id,
            Author = User,
            PublishedAt = DateTime.UtcNow,
            Status = PostStatus.Scheduled,
            Tags = [TagTest],
            Likes = [Admin]
        };

        public static Podcast Podcast => new Podcast
        {
            PodcastId = 1,
            Title = "Test podcast",
            AudioFile = "audio.mp3",
            CoverImage = "cover.jpg",
            Duration = 360,
            Bitrate = 128,
            Transcript = "test transcript"
        };

        public static Tag TagTest => new Tag { TagId = 1, Name = "test" };

        public static Comment Comment => new Comment
        {
            CommentId = 1,
            Content = "Comment_1",
            CreatedAt = DateTime.UtcNow,
            PostId = Post.PostId,
            Post = Post,
            UserId = User.Id,
            User = User,
            Status = CommentStatus.Pending,
            Replies = []
        };

        public static Notification Notification => new Notification
        {
            NotificationId = 1,
            Message = "Test notification",
            UserId = User.Id,
            User = User,
            IsRead = false
        };
    }
}
