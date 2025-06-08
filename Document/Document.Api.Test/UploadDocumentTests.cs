using Document.Api.Common.Interfaces;
using Document.Api.Domain.Events;
using Document.Api.Features.Documents;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Text;

namespace Document.Api.Test
{
    public class UploadDocumentQueryHandlerTest
    {
        private readonly Mock<IDocumentStorage> _storageMock;
        private readonly Mock<ICurrentUserService> _userServiceMock;
        private readonly UploadDocumentQueryHandler _handler;

        public UploadDocumentQueryHandlerTest()
        {
            _storageMock = new();
            _userServiceMock = new();
            _userServiceMock.Setup(u => u.UserId).Returns(Guid.Parse("5ae4677f-0d15-4572-ae18-597c1399f185"));

            _handler = new UploadDocumentQueryHandler(_storageMock.Object, _userServiceMock.Object);
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
            var file = CreateFakeFile();
            var query = new UploadDocumentQuery("Test Name", "Test Description", 1, file);

            _storageMock
                .Setup(s => s.AddDocument(It.IsAny<DocumentUploadedEvent>()))
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsError);
            Assert.IsType<Guid>(result.Value);
            Assert.NotEqual(Guid.Empty, result.Value);
        }

        [Fact]
        public async Task Handle_ShouldReturnError_WhenAddDocumentFails()
        {
            // Arrange
            var file = CreateFakeFile();
            var query = new UploadDocumentQuery("Test Name", "Test Description", 1, file);

            _storageMock
                .Setup(s => s.AddDocument(It.IsAny<DocumentUploadedEvent>()))
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
            var file = CreateFakeFile("custom_file.docx", "some content here");
            var query = new UploadDocumentQuery("Doc Title", "Doc Desc", 1, file);
            IDocumentEvent? capturedEvent = null;

            _storageMock
                .Setup(s => s.AddDocument(It.IsAny<DocumentUploadedEvent>()))
                .Callback<IDocumentEvent>(e => capturedEvent = e)
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(capturedEvent);
            Assert.NotEqual(Guid.Empty, capturedEvent.id);
            Assert.Equal(1, capturedEvent.Version);
        }
    }
}