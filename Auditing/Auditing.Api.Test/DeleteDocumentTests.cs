using Auditing.Api.Common.Interfaces;
using Auditing.Api.Domain.Events;
using Auditing.Api.Features.Auditings;
using Moq;
using Xunit;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Auditing.Api.Test
{
    public class DeleteAuditingQueryHandlerTest
    {
        private readonly Mock<IAuditingStorage> _storageMock;
        private readonly Mock<ICurrentUserService> _userServiceMock;
        private readonly DeleteAuditingQueryHandler _handler;

        public DeleteAuditingQueryHandlerTest()
        {
            _storageMock = new();
            _userServiceMock = new();
            _userServiceMock.Setup(u => u.UserId).Returns(Guid.Parse("5ae4677f-0d15-4572-ae18-597c1399f185"));

            _handler = new DeleteAuditingQueryHandler(_storageMock.Object, _userServiceMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnGuid_WhenDeleteAuditingSucceeds()
        {
            // Arrange
            var AuditingId = Guid.NewGuid();
            var query = new DeleteAuditingQuery(AuditingId);

            _storageMock
                .Setup(s => s.AddAuditing(It.IsAny<AuditingDeletedEvent>()))
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsError);
            Assert.Equal(AuditingId, result.Value);
        }

        [Fact]
        public async Task Handle_ShouldReturnError_WhenDeleteAuditingFails()
        {
            // Arrange
            var AuditingId = Guid.NewGuid();
            var query = new DeleteAuditingQuery(AuditingId);

            _storageMock
                .Setup(s => s.AddAuditing(It.IsAny<AuditingDeletedEvent>()))
                .ReturnsAsync(false);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal(Guid.Empty, result.Value);
            Assert.Contains(result.Errors, e => e.Code == "something went wrong trying so save the file.");
        }

        [Fact]
        public async Task Handle_ShouldCall_AddAuditing_WithCorrectEventData()
        {
            // Arrange
            var AuditingId = Guid.NewGuid();
            var query = new DeleteAuditingQuery(AuditingId);

            IAuditingEvent? capturedEvent = null;
            _storageMock
                .Setup(s => s.AddAuditing(It.IsAny<AuditingDeletedEvent>()))
                .Callback<IAuditingEvent>(e => capturedEvent = e)
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(capturedEvent);
            Assert.Equal(AuditingId, capturedEvent.Id);
            Assert.NotEqual(Guid.Empty, capturedEvent.Id);
        }
    }
}
