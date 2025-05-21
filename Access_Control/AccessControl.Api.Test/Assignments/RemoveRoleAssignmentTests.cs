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

namespace AccessControl.Api.Test.Assignments
{
    public class RemoveRoleAssignmentCommandHandlerTests
    {
        private readonly Mock<Context> _dbContextMock;
        private readonly RemoveRoleAssignmentCommandHandler _handler;
        private readonly Mock<ICurrentUserService> _userServiceMock;

        public RemoveRoleAssignmentCommandHandlerTests()
        {
            // Mock the ICurrentUserService
            _userServiceMock = new Mock<ICurrentUserService>();

            // Set up the DbContext with DbContextOptions and the mocked ICurrentUserService
            var optionsBuilder = new DbContextOptionsBuilder<Context>();
            optionsBuilder.UseInMemoryDatabase("AssignmentDbTest");

            _dbContextMock = new Mock<Context>(_userServiceMock.Object, optionsBuilder.Options);

            // Create the handler with the mock DbContext
            _handler = new RemoveRoleAssignmentCommandHandler(_dbContextMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnUnit_WhenRoleAssignmentExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var resourceId = Guid.NewGuid();
            var roleId = Guid.NewGuid();
            var assignment = new Assignment(userId, resourceId, new Role(roleId, "Admin", new List<Permission>()));

            var assignments = new List<Assignment> { assignment };
            _dbContextMock.Setup(db => db.Assignment)
                          .ReturnsDbSet(assignments.AsQueryable());

            var command = new RemoveAssignmentCommand(userId, resourceId, roleId);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsError);
            Assert.Equal(Unit.Value, result.Value); // Expecting successful removal
            _dbContextMock.Verify(db => db.Assignment.Remove(It.IsAny<Assignment>()), Times.Once); // Ensure removal was called
            _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once); // Ensure SaveChanges was called
        }

        [Fact]
        public async Task Handle_ShouldReturnError_WhenRoleAssignmentNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var resourceId = Guid.NewGuid();
            var roleId = Guid.NewGuid();

            var assignments = new List<Assignment>();
            _dbContextMock.Setup(db => db.Assignment)
                          .ReturnsDbSet(assignments.AsQueryable());

            var command = new RemoveAssignmentCommand(userId, resourceId, roleId);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsError); // Expecting an error
            Assert.Equal("Role assignment not found.", result.Errors.First().Code); // Check the error message
            _dbContextMock.Verify(db => db.Assignment.Remove(It.IsAny<Assignment>()), Times.Never); // Ensure remove was not called
            _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never); // Ensure SaveChanges was not called
        }

        [Fact]
        public async Task Handle_ShouldReturnError_WhenSaveChangesFails()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var resourceId = Guid.NewGuid();
            var roleId = Guid.NewGuid();
            var assignment = new Assignment(userId, resourceId, new Role(roleId, "Admin", new List<Permission>()));

            var assignments = new List<Assignment> { assignment };
            _dbContextMock.Setup(db => db.Assignment)
                          .ReturnsDbSet(assignments.AsQueryable());

            // Simulate a failure in SaveChangesAsync
            _dbContextMock.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
                          .ThrowsAsync(new Exception("Database save failed"));

            var command = new RemoveAssignmentCommand(userId, resourceId, roleId);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsError); // Expecting an error
            Assert.Equal("Database save failed", result.Errors.First().Code); // Check the error message
            _dbContextMock.Verify(db => db.Assignment.Remove(It.IsAny<Assignment>()), Times.Once); // Ensure removal was attempted
            _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once); // Ensure SaveChanges was called
        }
    }
}