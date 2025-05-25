using Document.Api.Common.Interfaces;
using Document.Api.Features.Documents;
using Microsoft.Extensions.Caching.Memory;
using Moq;

namespace Document.Api.Test
{
    public class GetDocumentQueryHandlerTest
    {
        private readonly Mock<IDocumentStorage> _storageMock;
        private readonly Mock<IMemoryCache> _memoryCacheMock;
        private readonly Mock<ICacheService> _cacheServiceMock;
        private readonly GetDocumentsWithPaginationQueryHandler _handler;

        public GetDocumentQueryHandlerTest()
        {
            _storageMock = new Mock<IDocumentStorage>();

            _memoryCacheMock = new Mock<IMemoryCache>();
            _cacheServiceMock = new Mock<ICacheService>();

            _cacheServiceMock = new Mock<ICacheService>();
            _handler = new GetDocumentsWithPaginationQueryHandler(_storageMock.Object, _cacheServiceMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnEmptyList_WhenNoEventsExist()
        {
            // Arrange
            _storageMock
                .Setup(s => s.GetDocumentList())
                .ReturnsAsync(new List<IDocumentEvent>());

            var query = new GetDocumentsWithPaginationQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsError);
            Assert.Empty(result.Value.Items);
        }

        [Fact]
        public async Task Handle_ShouldReturnDocuments_WithAppliedEvents()
        {
            // Arrange
            var documentId = Guid.NewGuid();
            var events = new List<IDocumentEvent>
            {
                new TestDocumentEvent(documentId, DateTime.UtcNow.AddMinutes(-10)),
                new TestDocumentEvent(documentId, DateTime.UtcNow)
            };

            _storageMock
                .Setup(s => s.GetDocumentList())
                .ReturnsAsync(events);

            var query = new GetDocumentsWithPaginationQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsError);
            Assert.Single(result.Value.Items);
        }

        [Fact]
        public async Task Handle_ShouldCall_SetCache_WhenDocumentsAreReturned()
        {
            // Arrange
            var documentId = Guid.NewGuid();
            var events = new List<IDocumentEvent>
            {
                new TestDocumentEvent(documentId, DateTime.UtcNow.AddMinutes(-10)),
                new TestDocumentEvent(documentId, DateTime.UtcNow)
            };

            _storageMock
                .Setup(s => s.GetDocumentList())
                .ReturnsAsync(events);

            // Setup cache mock behavior
            _cacheServiceMock.Setup(c => c.SetCache(It.IsAny<string>(), It.IsAny<object>()));

            var query = new GetDocumentsWithPaginationQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            _cacheServiceMock.Verify(c => c.SetCache(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        }

        private class TestDocumentEvent : IDocumentEvent
        {
            public Guid Id { get; }
            public DateTime OccurredAt { get; }

            public float? Version => throw new NotImplementedException();

            public TestDocumentEvent(Guid id, DateTime occurredAt)
            {
                Id = id;
                OccurredAt = occurredAt;
            }
        }
    }
}