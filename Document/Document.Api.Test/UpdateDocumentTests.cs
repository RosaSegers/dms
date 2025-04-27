using Document.Api.Common.Interfaces;
using Document.Api.Domain.Events;
using Document.Api.Features.Documents;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Document.Api.Test
{
    public class UpdateDocumentQueryHandlerTest
    {
        private readonly Mock<IDocumentStorage> _storageMock;
        private readonly Mock<ICurrentUserService> _userServiceMock;
        private readonly UpdateDocumentQueryHandler _handler;

        public UpdateDocumentQueryHandlerTest()
        {
            _storageMock = new();
            _userServiceMock = new();
            _userServiceMock.Setup(u => u.UserId).Returns(Guid.Parse("5ae4677f-0d15-4572-ae18-597c1399f185"));

            _handler = new UpdateDocumentQueryHandler(_storageMock.Object, _userServiceMock.Object);
        }

        private static IFormFile CreateFakeFile(string fileName = "uploaded_file.pdf", string content = "Fake file content")
        {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
            return new FormFile(stream, 0, stream.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "application/pdf"
            };
        }

        [Fact]
        public async Task Handle_ShouldReturnGuid_WhenAddDocumentSucceeds()
        {
            // Arrange
            var documentId = Guid.NewGuid();
            var mockFile = CreateFakeFile();

            var query = new UpdateDocumentQuery(documentId, "Updated Name", "Updated Description", 1, mockFile);

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
            var mockFile = CreateFakeFile();

            var query = new UpdateDocumentQuery(documentId, "Updated Name", "Updated Description", 1, mockFile);

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
            var mockFile = CreateFakeFile("custom_file.docx", "some content here");

            var query = new UpdateDocumentQuery(documentId, "Updated Name", "Updated Description", 1, mockFile);

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
            Assert.NotEqual(Guid.Empty, capturedEvent.Id);
        }
    }
}