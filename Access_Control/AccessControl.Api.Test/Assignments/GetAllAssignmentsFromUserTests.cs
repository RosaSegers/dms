using AccessControl.Api.Common.Interfaces;
using AccessControl.Api.Domain.Entities;
using AccessControl.Api.Features.Assignment;
using AccessControl.Api.Infrastructure.Persistance;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq.EntityFrameworkCore;
using static AccessControl.Api.Features.Assignment.GetAssignmentsController;

namespace AccessControl.Api.Test.Assignments
{
    public class GetAssignmentsQueryHandlerTests
    {
        private readonly Mock<Context> _dbContextMock;
        private readonly GetAssignmentsQueryHandler _handler;
        private readonly Mock<ICurrentUserService> _userServiceMock;

        public GetAssignmentsQueryHandlerTests()
        {
            _userServiceMock = new Mock<ICurrentUserService>();
            var optionsBuilder = new DbContextOptionsBuilder<Context>();
            optionsBuilder.UseInMemoryDatabase("AssignmentDbTest");

            _dbContextMock = new Mock<Context>(_userServiceMock.Object, optionsBuilder.Options);

            _handler = new GetAssignmentsQueryHandler(_dbContextMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnAssignments_WhenUserHasAssignments()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var resourceId = Guid.NewGuid();
            var roleId = Guid.NewGuid();
            var assignment = new Assignment(userId, resourceId, new Role("Admin", new List<Permission>()));

            var assignments = new List<Assignment> { assignment };
            _dbContextMock.Setup(db => db.Assignment)
                          .ReturnsDbSet(assignments.AsQueryable());

            var query = new GetAssignmentsQuery(userId, resourceId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsError);
            Assert.Single(result.Value); // Expecting one assignment
            Assert.Equal(userId, result.Value[0].UserId);
        }

        [Fact]
        public async Task Handle_ShouldReturnEmptyList_WhenUserHasNoAssignments()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var resourceId = Guid.NewGuid();
            var query = new GetAssignmentsQuery(userId, resourceId);

            _dbContextMock.Setup(db => db.Assignment)
                          .ReturnsDbSet(new List<Assignment>().AsQueryable());

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsError);
            Assert.Empty(result.Value); // Expecting an empty list
        }

        [Fact]
        public async Task Handle_ShouldFilterAssignmentsByResourceId_WhenResourceIdIsProvided()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var resourceId1 = Guid.NewGuid();
            var resourceId2 = Guid.NewGuid();
            var assignment1 = new Assignment(userId, resourceId1, new Role("Admin", new List<Permission>()));
            var assignment2 = new Assignment(userId, resourceId2, new Role("Admin", new List<Permission>()));

            var assignments = new List<Assignment> { assignment1, assignment2 };
            _dbContextMock.Setup(db => db.Assignment)
                          .ReturnsDbSet(assignments.AsQueryable());

            var query = new GetAssignmentsQuery(userId, resourceId1);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsError);
            Assert.Single(result.Value); // Expecting only the first assignment
            Assert.Equal(resourceId1, result.Value[0].ResourceId);
        }

        [Fact]
        public async Task Handle_ShouldReturnEmptyList_WhenNoAssignmentsMatchTheCriteria()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var resourceId = Guid.NewGuid();
            var query = new GetAssignmentsQuery(userId, resourceId);

            _dbContextMock.Setup(db => db.Assignment)
                          .ReturnsDbSet(new List<Assignment>().AsQueryable());

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsError);
            Assert.Empty(result.Value); // Expecting an empty list as no assignments match
        }
    }
}