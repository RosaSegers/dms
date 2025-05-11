using Auditing.Api.Common.Interfaces;
using Auditing.Api.Domain.Events;
using Auditing.Api.Features.Auditings;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auditing.Api.Test
{
    public class RolebackAuditingTests
    {
        private readonly Mock<IAuditingStorage> _storageMock;
        private readonly Mock<ICurrentUserService> _userServiceMock;
        private readonly RolebackAuditingQueryHandler _handler;

        public RolebackAuditingTests()
        {
            _storageMock = new Mock<IAuditingStorage>();
            _userServiceMock = new Mock<ICurrentUserService>();
            _userServiceMock.Setup(u => u.UserId).Returns(Guid.NewGuid());

            _handler = new RolebackAuditingQueryHandler(_storageMock.Object, _userServiceMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnId_WhenAddAuditingSucceeds()
        {
            // Arrange
            var docId = Guid.NewGuid();
            var query = new RolebackAuditingQuery(docId, Version: 3);

            var versions = new List<IAuditingEvent>
            {
                new AuditingVersion {Version = 1},
                new AuditingVersion {Version = 2},
                new AuditingVersion {Version = 3},
                new AuditingVersion {Version = 4}
            };

            _storageMock
                .Setup(s => s.GetAuditingById(docId))
                .ReturnsAsync(versions);

            _storageMock
                .Setup(s => s.AddAuditing(It.IsAny<AuditingRolebackEvent>()))
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsError);
            Assert.IsType<Guid>(result.Value);
            _storageMock.Verify(s => s.AddAuditing(It.Is<AuditingRolebackEvent>(e =>
                e.Id != Guid.Empty &&
                e.Version == 3
            )), Times.Once);
        }
    }

    internal class AuditingVersion : IAuditingEvent
    {
        public Guid Id { get; set; }
        public float? Version { get; set; }
        public DateTime OccurredAt { get; set; }
    }
}
