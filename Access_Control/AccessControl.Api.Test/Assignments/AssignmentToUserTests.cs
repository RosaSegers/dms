using AccessControl.Api.Common.Interfaces;
using AccessControl.Api.Domain.Entities;
using AccessControl.Api.Features.Assignment;
using AccessControl.Api.Infrastructure.Persistance;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;

namespace AccessControl.Api.Test.Assignments
{
    public class AssignAssignmentToUserCommandHandlerTests
    {
        private readonly Mock<Context> _dbContextMock;
        private readonly Mock<ICurrentUserService> _userService;
        private readonly AssignAssignmentToUserCommandHandler _handler;

        public AssignAssignmentToUserCommandHandlerTests()
        {
            var optionsBuilder = new DbContextOptionsBuilder<Context>();
            optionsBuilder.UseInMemoryDatabase("AssignmentDbTest");

            _userService = new();
            _dbContextMock = new(_userService.Object, optionsBuilder.Options);
            _handler = new AssignAssignmentToUserCommandHandler(_dbContextMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnUnit_WhenAssignmentIsCreatedSuccessfully()
        {
            // Arrange
            var command = new AssignAssignmentToUserCommand(
                UserId: Guid.NewGuid(),
                ResourceId: Guid.NewGuid(),
                RoleId: Guid.NewGuid()
            );

            var dbSetMock = new Mock<DbSet<Assignment>>();
            _dbContextMock.Setup(db => db.Assignment).Returns(dbSetMock.Object);
            _dbContextMock.Setup(db => db.Roles).ReturnsDbSet(new List<Role>
            {
                new Role
                {
                    Id = command.RoleId,
                    Name = "Admin",
                    Permissions = new List<Permission>()
                }
            }.AsQueryable());
            dbSetMock.Setup(d => d.AddAsync(It.IsAny<Assignment>(), It.IsAny<CancellationToken>()));

            _dbContextMock.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
                          .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsError);
            Assert.Equal(Unit.Value, result.Value);
        }
    }
}