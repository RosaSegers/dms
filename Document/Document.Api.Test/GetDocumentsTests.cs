using Document.Api.Common.Interfaces;
using Document.Api.Domain.Events;
using Document.Api.Features.Documents;
using Document.Api.Infrastructure.Persistance;
using Moq;
using Xunit;

namespace Document.Api.Test
{
    public class GetDocumentQueryHandlerTest
    {
        private readonly Mock<IDocumentStorage> _storageMock;
        private readonly GetDocumentQueryHandler _handler;

        public GetDocumentQueryHandlerTest()
        {
            _storageMock = new();
            _handler = new GetDocumentQueryHandler(_storageMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnEmptyList_WhenNoEventsExist()
        {
            // Arrange
            _storageMock
                .Setup(s => s.GetDocumentList())
                .ReturnsAsync(new List<IDocumentEvent>());

            var query = new GetDocumentQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsError);
            Assert.Empty(result.Value);
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

            var query = new GetDocumentQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsError);
            Assert.Single(result.Value);
        }

        [Fact]
        public async Task Handle_ShouldGroupEvents_ByDocumentId()
        {
            // Arrange
            var doc1 = Guid.NewGuid();
            var doc2 = Guid.NewGuid();

            var events = new List<IDocumentEvent>
            {
                new TestDocumentEvent(doc1, DateTime.UtcNow.AddMinutes(-20)),
                new TestDocumentEvent(doc2, DateTime.UtcNow.AddMinutes(-10)),
                new TestDocumentEvent(doc1, DateTime.UtcNow),
                new TestDocumentEvent(doc2, DateTime.UtcNow)
            };

            _storageMock
                .Setup(s => s.GetDocumentList())
                .ReturnsAsync(events);

            var query = new GetDocumentQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsError);
            Assert.Equal(2, result.Value.Count);
        }

        // Dummy test implementation of IDocumentEvent
        private class TestDocumentEvent : IDocumentEvent
        {
            public Guid Id { get; }
            public DateTime OccurredAt { get; }

            public TestDocumentEvent(Guid id, DateTime occurredAt)
            {
                Id = id;
                OccurredAt = occurredAt;
            }
        }
    }
}