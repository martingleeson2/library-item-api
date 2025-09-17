using Example.LibraryItem.Application.Interfaces;
using Example.LibraryItem.Application.Services;
using Example.LibraryItem.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Example.LibraryItem.Tests.Helpers
{
    public static class TestHelpers
    {
        public static LibraryDbContext CreateInMemoryDb()
        {
            var options = new DbContextOptionsBuilder<LibraryDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new LibraryDbContext(options);
        }

        public static IItemValidationService CreateValidationService(LibraryDbContext db)
        {
            return new ItemValidationService(db, NullLogger<ItemValidationService>.Instance);
        }

        public static IDateTimeProvider CreateTestDateTimeProvider()
        {
            return new TestDateTimeProvider();
        }

        public static IUserContext CreateTestUserContext(string? user = "test-user")
        {
            var mock = new Mock<IUserContext>();
            mock.Setup(x => x.CurrentUser).Returns(user);
            return mock.Object;
        }

        private class TestDateTimeProvider : IDateTimeProvider
        {
            public DateTime UtcNow => new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        }
    }
}