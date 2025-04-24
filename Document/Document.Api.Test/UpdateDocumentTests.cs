using Document.Api.Common.Interfaces;
using Document.Api.Domain.Events;
using Document.Api.Features.Documents;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace Document.Api.Test
{
    public class UpdateDocumentQueryHandlerTest
    {
        private readonly Mock<IDocumentStorage> _storageMock;
        private readonly UpdateDocumentQueryHandler _handler;

        public UpdateDocumentQueryHandlerTest()
        {
            _storageMock = new();
            _handler = new UpdateDocumentQueryHandler(_storageMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnGuid_WhenAddDocumentSucceeds()
        {
            // Arrange
            var documentId = Guid.NewGuid();
            var mockFile = new Mock<IFormFile>();

            var query = new UpdateDocumentQuery(documentId, "Updated Name", "Updated Description", mockFile.Object);

            _storageMock
                .Setup(s => s.AddDocument(It.IsAny<DocumentUpdatedEvent>()))
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
            var mockFile = new Mock<IFormFile>();

            var query = new UpdateDocumentQuery(documentId, "Updated Name", "Updated Description", mockFile.Object);

            _storageMock
                .Setup(s => s.AddDocument(It.IsAny<DocumentUpdatedEvent>()))
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
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns("file.pdf");

            var query = new UpdateDocumentQuery(documentId, "Updated Name", "Updated Description", mockFile.Object);

            IDocumentEvent? capturedEvent = null;
            _storageMock
                .Setup(s => s.AddDocument(It.IsAny<DocumentUpdatedEvent>()))
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