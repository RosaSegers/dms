using Document.Api.Common.Interfaces;
using Document.Api.Domain.Events;
using Document.Api.Features.Documents;
using Document.Api.Infrastructure.Persistance;
using Moq;

namespace Document.Api.Test
{
    public class DeleteDocumentQueryHandlerTest
    {
        private readonly Mock<IDocumentStorage> _storageMock;
        private readonly DeleteDocumentQueryHandler _handler;

        public DeleteDocumentQueryHandlerTest()
        {
            _storageMock = new();
            _handler = new DeleteDocumentQueryHandler(_storageMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnGuid_WhenAddDocumentSucceeds()
        {
            // Arrange
            var documentId = Guid.NewGuid();
            var query = new DeleteDocumentQuery(documentId);

            _storageMock
                .Setup(s => s.AddDocument(It.IsAny<DocumentDeletedEvent>()))
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsError);
            Assert.Equal(documentId, result.Value);
        }

        [Fact]
        public async Task Handle_ShouldReturnError_WhenAddDocumentFails()
        {
            // Arrange
            var documentId = Guid.NewGuid();
            var query = new DeleteDocumentQuery(documentId);

            _storageMock
                .Setup(s => s.AddDocument(It.IsAny<DocumentDeletedEvent>()))
                .ReturnsAsync(false);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal(Guid.Empty, result.Value);
            Assert.Contains(result.Errors, e => e.Code == "something went wrong trying so save the file.");
        }

        [Fact]
        public async Task Handle_ShouldCall_AddDocument_WithCorrectEventData()
        {
            // Arrange
            var documentId = Guid.NewGuid();
            var query = new DeleteDocumentQuery(documentId);

            IDocumentEvent capturedEvent = null!;
            _storageMock
                .Setup(s => s.AddDocument(It.IsAny<DocumentDeletedEvent>()))
                .Callback<IDocumentEvent>(e => capturedEvent = e)
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(capturedEvent);
            Assert.Equal(documentId, capturedEvent.Id);
        }
    }
}