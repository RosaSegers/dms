using AccessControl.Api.Common.Interfaces;
using AccessControl.Api.Domain.Entities;
using AccessControl.Api.Features.Assignment;
using AccessControl.Api.Infrastructure.Persistance;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;

namespace AccessControl.Api.Test.Assignments
{
    public class RemoveRoleAssignmentCommandHandlerTests
    {
        private readonly Context _dbContext;
        private readonly RemoveRoleAssignmentCommandHandler _handler;

        public RemoveRoleAssignmentCommandHandlerTests()
        {
            var options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var userServiceMock = new Mock<ICurrentUserService>();
            _dbContext = new Context(userServiceMock.Object, options);
            _handler = new RemoveRoleAssignmentCommandHandler(_dbContext);
        }

        [Fact]
        public async Task Handle_ShouldReturnUnit_WhenAssignmentIsRemovedSuccessfully()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var resourceId = Guid.NewGuid();
            var roleId = Guid.NewGuid();
            var role = new Role(roleId, "", new List<Permission>());

            var assignment = new Assignment(userId, resourceId, role);

            _dbContext.Assignment.Add(assignment);
            await _dbContext.SaveChangesAsync();

            var command = new RemoveAssignmentCommand(userId, resourceId, roleId);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsError);
            Assert.Equal(Unit.Value, result.Value);

            var assignmentInDb = await _dbContext.Assignment
                .FirstOrDefaultAsync(a => a.UserId == userId && a.ResourceId == resourceId && a.Role.Id == roleId);
            Assert.Null(assignmentInDb); // Ensure it was removed
        }

        [Fact]
        public async Task Handle_ShouldReturnNotFoundError_WhenAssignmentDoesNotExist()
        {
            // Arrange
            var command = new RemoveAssignmentCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("Role assignment not found.", result.FirstError.Code);
            Assert.Equal(ErrorType.NotFound, result.FirstError.Type);
        }
    }
}