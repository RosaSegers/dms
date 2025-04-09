using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;
using User.Api.Features.Users;
using User.Api.Infrastructure.Persistance;
using User.API.Common.Interfaces;

namespace User.Api.Test
{
    public class GetUsersTest
    {
        private readonly Mock<UserDatabaseContext> _dbContext;
        private readonly Mock<ICurrentUserService> _userService;

        public GetUsersTest()
        {
            var entities = new List<Domain.Entities.User>();
            for(int i = 0; i < 100; i++)
            {
                entities.Add(new Domain.Entities.User($"Rosa{i}", $"Rosa{i}@Email.com", "123123asdasdASDASD"));
            }

            var optionsBuilder = new DbContextOptionsBuilder<UserDatabaseContext>();
            optionsBuilder.UseInMemoryDatabase("Users");

            _userService = new();
            _dbContext = new Mock<UserDatabaseContext>(_userService.Object, optionsBuilder.Options);
            _dbContext.Setup(p => p.Users).ReturnsDbSet(entities.AsQueryable());
            _dbContext.Setup(p => p.SaveChanges()).Returns(1);
        }

        [Fact]
        public async Task Handle_Should_ReturnTenRecords_WhenPaginationIsNotSet()
        {
            // Arrange
            var command = new GetUsersWithPaginationQuery();
            var handler = new GetUserItemsWithPaginationQueryHandler(_dbContext.Object);

            // Act
            var result = await handler.Handle(command, new CancellationToken());
            
            //Assert
            Assert.Equal(10, result.Value.Items.Count);
            Assert.NotEqual(result.Value.Items.Count, result.Value.TotalCount);

            Assert.False(result.Value.HasPreviousPage);
            Assert.True(result.Value.HasNextPage);

            Assert.True(result.Value.PageNumber >= 1);
        }

        [Fact]
        public async Task Handle_Should_ReturnTwoRecords_WhenPageSizeIsSetToTwo()
        {
            // Arrange
            var command = new GetUsersWithPaginationQuery(PageSize: 2);
            var handler = new GetUserItemsWithPaginationQueryHandler(_dbContext.Object);

            // Act
            var result = await handler.Handle(command, new CancellationToken());

            //Assert
            Assert.Equal(2, result.Value.Items.Count);
            Assert.NotEqual(result.Value.Items.Count, result.Value.TotalCount);

            Assert.False(result.Value.HasPreviousPage);
            Assert.True(result.Value.HasNextPage);

            Assert.True(result.Value.PageNumber >= 1);
        }

        [Fact]
        public async Task Handle_Should_ReturnPageTwo_WhenPageNumberIsSetToTwo()
        {
            // Arrange
            var command = new GetUsersWithPaginationQuery(PageNumber: 2);
            var handler = new GetUserItemsWithPaginationQueryHandler(_dbContext.Object);

            // Act
            var result = await handler.Handle(command, new CancellationToken());

            //Assert
            Assert.Equal(10, result.Value.Items.Count);
            Assert.NotEqual(result.Value.Items.Count, result.Value.TotalCount);

            Assert.Equal(2, result.Value.PageNumber);
            Assert.True(result.Value.HasPreviousPage);
            Assert.True(result.Value.HasNextPage);

            Assert.True(result.Value.PageNumber >= 1);
        }

        [Fact]
        public async Task Handle_Should_ReturnEmptyList_WhenNoUsersExist()
        {
            // Arrange
            var emptyDbContextMock = new Mock<UserDatabaseContext>(_userService.Object, new DbContextOptions<UserDatabaseContext>());
            emptyDbContextMock.Setup(p => p.Users).ReturnsDbSet(Enumerable.Empty<Domain.Entities.User>().AsQueryable());

            var command = new GetUsersWithPaginationQuery();
            var handler = new GetUserItemsWithPaginationQueryHandler(emptyDbContextMock.Object);

            // Act
            var result = await handler.Handle(command, new CancellationToken());

            // Assert
            Assert.Empty(result.Value.Items);  // Ensure no users are returned
            Assert.Equal(0, result.Value.TotalCount);  // TotalCount should be 0
        }
    }
}
