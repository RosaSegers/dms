using AccessControl.Api.Common.Interfaces;
using AccessControl.Api.Domain.Entities;
using AccessControl.Api.Features.Assignment;
using AccessControl.Api.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;
using Moq;
using static AccessControl.Api.Features.Assignment.GetAssignmentsController;

namespace AccessControl.Api.Test.Assignments
{
    public class GetAssignmentsQueryHandlerTests
    {
        private readonly Context _dbContext;
        private readonly GetAssignmentsQueryHandler _handler;

        public GetAssignmentsQueryHandlerTests()
        {
            var options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) // Unique db for each test
                .Options;

            var currentUserServiceMock = new Mock<ICurrentUserService>();
            _dbContext = new Context(currentUserServiceMock.Object, options);

            _handler = new GetAssignmentsQueryHandler(_dbContext);
        }

        [Fact]
        public async Task Handle_ShouldReturnAssignments_WhenUserIdMatches()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var resourceId = Guid.NewGuid();
            var roleId = Guid.NewGuid();

            var assignments = new List<Assignment>
            {
                new Assignment(userId, resourceId, new Role(roleId, "", new())),
                new Assignment(userId, Guid.NewGuid(), new Role(Guid.NewGuid(), "", new())) // different ResourceId
            };

            await _dbContext.Assignment.AddRangeAsync(assignments);
            await _dbContext.SaveChangesAsync();

            var query = new GetAssignmentsQuery(userId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsError);
            Assert.Equal(2, result.Value.Count); // Should return 2 assignments (userId matches)
        }

        [Fact]
        public async Task Handle_ShouldReturnFilteredAssignments_WhenResourceIdIsProvided()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var resourceId = Guid.NewGuid();
            var roleId = Guid.NewGuid();

            var assignments = new List<Assignment>
            {
                new Assignment(userId, resourceId, new Role(roleId, "", new())),
                new Assignment(userId, Guid.NewGuid(), new Role(Guid.NewGuid(), "", new())) // different ResourceId
            };

            await _dbContext.Assignment.AddRangeAsync(assignments);
            await _dbContext.SaveChangesAsync();

            var query = new GetAssignmentsQuery(userId)
            {
                ResourceId = resourceId // Filtering by ResourceId
            };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsError);
            Assert.Single(result.Value); // Should return only one assignment matching the resourceId
            Assert.Equal(resourceId, result.Value.First().ResourceId);
        }

        [Fact]
        public async Task Handle_ShouldReturnEmptyList_WhenNoAssignmentsMatch()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var query = new GetAssignmentsQuery(userId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsError);
            Assert.Empty(result.Value); // Should return an empty list as no assignments match the userId
        }
    }
}