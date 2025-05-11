using Auditing.Api.Common.Constants;
using Auditing.Api.Common.Interfaces;
using Auditing.Api.Features.Auditings;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auditing.Api.Test
{
    public class GetAuditingByIdTests
    {
        private readonly Mock<IAuditingStorage> _storageMock;
        private readonly Mock<ICacheService> _cacheMock;
        private readonly GetAuditingByIdQueryHandler _handler;

        public GetAuditingByIdTests()
        {
            _storageMock = new Mock<IAuditingStorage>();
            _cacheMock = new Mock<ICacheService>();
            _handler = new GetAuditingByIdQueryHandler(_storageMock.Object, _cacheMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnCachedAuditing_WhenAuditingExistsInCache()
        {
            // Arrange
            var AuditingId = Guid.NewGuid();
            var cacheKey = CacheKeys.GetAuditingCacheKey(AuditingId);
            object cachedAuditing = new Domain.Entities.Auditing();

            _cacheMock
                .Setup(c => c.TryGetCache(cacheKey, out cachedAuditing))
                .Returns(true);

            var query = new GetAuditingByIdQuery(AuditingId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsError);
            Assert.Equal(cachedAuditing, result.Value);
            _storageMock.Verify(s => s.GetAuditingList(), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldQueryStorageAndCacheAuditing_WhenAuditingNotInCache()
        {
            // Arrange
            var AuditingId = Guid.NewGuid();
            var query = new GetAuditingByIdQuery(AuditingId);

            var events = new List<IAuditingEvent>
            {
                new TestAuditingEvent(AuditingId, DateTime.UtcNow.AddMinutes(-10)),
                new TestAuditingEvent(AuditingId, DateTime.UtcNow)
            };

            var Auditing = new Domain.Entities.Auditing();
            foreach (var e in events.OrderBy(e => e.OccurredAt))
            {
                Auditing.Apply(e);
            }

            _cacheMock
                .Setup(c => c.TryGetCache(It.IsAny<string>(), out It.Ref<object>.IsAny))
                .Returns(false);

            _storageMock
                .Setup(s => s.GetAuditingList())
                .ReturnsAsync(events);

            _cacheMock
                .Setup(c => c.SetCache(It.IsAny<string>(), It.IsAny<object>()))
                .Verifiable(); 

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsError);
            Assert.Equal(Auditing.Id, result.Value.Id);
            _cacheMock.Verify(c => c.SetCache(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnEmptyAuditing_WhenNoEventsFound()
        {
            // Arrange
            var AuditingId = Guid.NewGuid();
            var query = new GetAuditingByIdQuery(AuditingId);

            _storageMock
                .Setup(s => s.GetAuditingList())
                .ReturnsAsync(new List<IAuditingEvent>());

            _cacheMock
                .Setup(c => c.TryGetCache(It.IsAny<string>(), out It.Ref<object>.IsAny))
                .Returns(false);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsError);
            Assert.NotNull(result.Value);
            Assert.IsType<Domain.Entities.Auditing>(result.Value);
        }

        [Fact]
        public async Task Handle_ShouldApplyEventsInOrder_WhenMultipleEventsExist()
        {
            // Arrange
            var AuditingId = Guid.NewGuid();
            var query = new GetAuditingByIdQuery(AuditingId);

            var events = new List<IAuditingEvent>
            {
                new TestAuditingEvent(AuditingId, DateTime.UtcNow.AddMinutes(-10)),
                new TestAuditingEvent(AuditingId, DateTime.UtcNow)
            };

            var Auditing = new Domain.Entities.Auditing();
            foreach (var e in events.OrderBy(e => e.OccurredAt))
            {
                Auditing.Apply(e);
            }

            _cacheMock
                .Setup(c => c.TryGetCache(It.IsAny<string>(), out It.Ref<object>.IsAny))
                .Returns(false);

            _storageMock
                .Setup(s => s.GetAuditingList())
                .ReturnsAsync(events);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsError);
            Assert.Equal(Auditing.Id, result.Value.Id);
        }

        private class TestAuditingEvent : IAuditingEvent
        {
            public Guid Id { get; }
            public DateTime OccurredAt { get; }

            public float? Version => throw new NotImplementedException();

            public TestAuditingEvent(Guid id, DateTime occurredAt)
            {
                Id = id;
                OccurredAt = occurredAt;
            }
        }
    }
}
