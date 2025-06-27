using Microsoft.AspNetCore.Identity;
using Moq;

namespace PodcastBlog.Tests.TestUtils
{
    public static class MockUserManager
    {
        public static Mock<UserManager<TUser>> Create<TUser>() where TUser : class
        {
            var store = new Mock<IUserStore<TUser>>();

            return new Mock<UserManager<TUser>>(store.Object, null, null, null, null, null, null, null, null);
        }
    }
}
