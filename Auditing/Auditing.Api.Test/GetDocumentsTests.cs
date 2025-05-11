using Auditing.Api.Common.Interfaces;
using Auditing.Api.Domain.Events;
using Auditing.Api.Features.Auditings;
using Auditing.Api.Infrastructure.Persistance;
using Moq;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Auditing.Api.Test
{
    public class GetAuditingQueryHandlerTest
    {
        private readonly Mock<IAuditingStorage> _storageMock;
        private readonly Mock<IMemoryCache> _memoryCacheMock;
        private readonly Mock<ICacheService> _cacheServiceMock;
        private readonly GetAuditingsWithPaginationQueryHandler _handler;

        public GetAuditingQueryHandlerTest()
        {
            _storageMock = new Mock<IAuditingStorage>();

            _memoryCacheMock = new Mock<IMemoryCache>();
            _cacheServiceMock = new Mock<ICacheService>();

            _cacheServiceMock = new Mock<ICacheService>();
            _handler = new GetAuditingsWithPaginationQueryHandler(_storageMock.Object, _cacheServiceMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnEmptyList_WhenNoEventsExist()
        {
            // Arrange
            _storageMock
                .Setup(s => s.GetAuditingList())
                .ReturnsAsync(new List<IAuditingEvent>());

            var query = new GetAuditingsWithPaginationQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsError);
            Assert.Empty(result.Value.Items);
        }

        [Fact]
        public async Task Handle_ShouldReturnAuditings_WithAppliedEvents()
        {
            // Arrange
            var AuditingId = Guid.NewGuid();
            var events = new List<IAuditingEvent>
            {
                new TestAuditingEvent(AuditingId, DateTime.UtcNow.AddMinutes(-10)),
                new TestAuditingEvent(AuditingId, DateTime.UtcNow)
            };

            _storageMock
                .Setup(s => s.GetAuditingList())
                .ReturnsAsync(events);

            var query = new GetAuditingsWithPaginationQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsError);
            Assert.Single(result.Value.Items);
        }

        [Fact]
        public async Task Handle_ShouldCall_SetCache_WhenAuditingsAreReturned()
        {
            // Arrange
            var AuditingId = Guid.NewGuid();
            var events = new List<IAuditingEvent>
            {
                new TestAuditingEvent(AuditingId, DateTime.UtcNow.AddMinutes(-10)),
                new TestAuditingEvent(AuditingId, DateTime.UtcNow)
            };

            _storageMock
                .Setup(s => s.GetAuditingList())
                .ReturnsAsync(events);

            // Setup cache mock behavior
            _cacheServiceMock.Setup(c => c.SetCache(It.IsAny<string>(), It.IsAny<object>()));

            var query = new GetAuditingsWithPaginationQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            _cacheServiceMock.Verify(c => c.SetCache(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
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
