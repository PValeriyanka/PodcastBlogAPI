namespace PodcastBlog.Infrastructure.ExceptionsHandler.Exceptions
{
    public class AuthException : Exception
    {
        public AuthException(string message) : base(message) { }
    }
}
