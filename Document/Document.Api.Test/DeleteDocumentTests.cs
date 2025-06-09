using Document.Api.Common.Interfaces;
using Document.Api.Domain.Events;
using Document.Api.Features.Documents;
using Moq;
using Xunit;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Document.Api.Test
{
    public class DeleteDocumentQueryHandlerTest
    {
        private readonly Mock<IDocumentStorage> _storageMock;
        private readonly Mock<ICurrentUserService> _userServiceMock;
        private readonly DeleteDocumentCommandHandler _handler;

        public DeleteDocumentQueryHandlerTest()
        {
            _storageMock = new();
            _userServiceMock = new();
            _userServiceMock.Setup(u => u.UserId).Returns(Guid.Parse("5ae4677f-0d15-4572-ae18-597c1399f185"));

            _handler = new DeleteDocumentCommandHandler(_storageMock.Object, _userServiceMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnGuid_WhenDeleteDocumentSucceeds()
        {
            // Arrange
            var documentId = Guid.NewGuid();
            var query = new DeleteDocumentCommand(documentId);

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
        public async Task Handle_ShouldReturnError_WhenDeleteDocumentFails()
        {
            // Arrange
            var documentId = Guid.NewGuid();
            var query = new DeleteDocumentCommand(documentId);

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
            var query = new DeleteDocumentCommand(documentId);

            IDocumentEvent? capturedEvent = null;
            _storageMock
                .Setup(s => s.AddDocument(It.IsAny<DocumentDeletedEvent>()))
                .Callback<IDocumentEvent>(e => capturedEvent = e)
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(capturedEvent);
            Assert.Equal(documentId, capturedEvent.DocumentId);
            Assert.NotEqual(Guid.Empty, capturedEvent.DocumentId);
        }
    }
}