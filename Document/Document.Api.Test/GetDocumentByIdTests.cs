using Document.Api.Common.Constants;
using Document.Api.Common.Interfaces;
using Document.Api.Features.Documents;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Document.Api.Test
{
    public class GetDocumentByIdTests
    {
        private readonly Mock<IDocumentStorage> _storageMock;
        private readonly Mock<ICacheService> _cacheMock;
        private readonly Mock<ICurrentUserService> _userService;
        private readonly GetDocumentByIdQueryHandler _handler;

        public GetDocumentByIdTests()
        {
            _storageMock = new Mock<IDocumentStorage>();
            _cacheMock = new Mock<ICacheService>();
            _userService = new Mock<ICurrentUserService>();
            _handler = new GetDocumentByIdQueryHandler(_storageMock.Object, _userService.Object, _cacheMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnCachedDocument_WhenDocumentExistsInCache()
        {
            // Arrange
            var documentId = Guid.NewGuid();
            var cacheKey = CacheKeys.GetDocumentCacheKey(documentId);
            object cachedDocument = new Domain.Entities.Document();

            _cacheMock
                .Setup(c => c.TryGetCache(cacheKey, out cachedDocument))
                .Returns(true);

            var query = new GetDocumentByIdQuery(documentId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsError);
            Assert.Equal(cachedDocument, result.Value);
            _storageMock.Verify(s => s.GetDocumentList(), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldQueryStorageAndCacheDocument_WhenDocumentNotInCache()
        {
            // Arrange
            var documentId = Guid.NewGuid();
            var query = new GetDocumentByIdQuery(documentId);

            var events = new List<IDocumentEvent>
            {
                new TestDocumentEvent(documentId, DateTime.UtcNow.AddMinutes(-10)),
                new TestDocumentEvent(documentId, DateTime.UtcNow)
            };

            var document = new Domain.Entities.Document();
            foreach (var e in events.OrderBy(e => e.OccurredAt))
            {
                document.Apply(e);
            }

            _cacheMock
                .Setup(c => c.TryGetCache(It.IsAny<string>(), out It.Ref<object>.IsAny))
                .Returns(false);

            _storageMock
                .Setup(s => s.GetDocumentList())
                .ReturnsAsync(events);

            _cacheMock
                .Setup(c => c.SetCache(It.IsAny<string>(), It.IsAny<object>()))
                .Verifiable(); 

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsError);
            Assert.Equal(document.Id, result.Value.Id);
            _cacheMock.Verify(c => c.SetCache(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnEmptyDocument_WhenNoEventsFound()
        {
            // Arrange
            var documentId = Guid.NewGuid();
            var query = new GetDocumentByIdQuery(documentId);

            _storageMock
                .Setup(s => s.GetDocumentList())
                .ReturnsAsync(new List<IDocumentEvent>());

            _cacheMock
                .Setup(c => c.TryGetCache(It.IsAny<string>(), out It.Ref<object>.IsAny))
                .Returns(false);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsError);
            Assert.NotNull(result.Value);
            Assert.IsType<Domain.Entities.Document>(result.Value);
        }

        [Fact]
        public async Task Handle_ShouldApplyEventsInOrder_WhenMultipleEventsExist()
        {
            // Arrange
            var documentId = Guid.NewGuid();
            var query = new GetDocumentByIdQuery(documentId);

            var events = new List<IDocumentEvent>
            {
                new TestDocumentEvent(documentId, DateTime.UtcNow.AddMinutes(-10)),
                new TestDocumentEvent(documentId, DateTime.UtcNow)
            };

            var document = new Domain.Entities.Document();
            foreach (var e in events.OrderBy(e => e.OccurredAt))
            {
                document.Apply(e);
            }

            _cacheMock
                .Setup(c => c.TryGetCache(It.IsAny<string>(), out It.Ref<object>.IsAny))
                .Returns(false);

            _storageMock
                .Setup(s => s.GetDocumentList())
                .ReturnsAsync(events);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsError);
            Assert.Equal(document.Id, result.Value.Id);
        }

        private class TestDocumentEvent : IDocumentEvent
        {
            public Guid DocumentId { get; }
            public DateTime OccurredAt { get; }

            public int? Version => throw new NotImplementedException();

            public string EventType => throw new NotImplementedException();

            public string Id => throw new NotImplementedException();

            public TestDocumentEvent(Guid id, DateTime occurredAt)
            {
                this.DocumentId = id;
                OccurredAt = occurredAt;
            }
        }
    }
}
