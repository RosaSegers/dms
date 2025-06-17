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
using Document.Api.Infrastructure.Background.Interfaces;

namespace Document.Api.Test
{
    public class UpdateDocumentCommandHandlerTest
    {
        private readonly Mock<IDocumentScanQueue> _queueMock;
        private readonly Mock<ICurrentUserService> _userServiceMock;
        private readonly UpdateDocumentCommandHandler _handler;

        public UpdateDocumentCommandHandlerTest()
        {
            _queueMock = new();
            _userServiceMock = new();
            _userServiceMock.Setup(u => u.UserId).Returns(Guid.Parse("5ae4677f-0d15-4572-ae18-597c1399f185"));

            _handler = new UpdateDocumentCommandHandler(_queueMock.Object, _userServiceMock.Object);
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
        public async Task Handle_ShouldCloneStream_ForSafety()
        {
            // Arrange
            var documentId = Guid.NewGuid();
            var mockFile = CreateFakeFile("file.txt", "hello world!");

            DocumentScanQueueItem? capturedItem = null;

            _queueMock
                .Setup(q => q.Enqueue(It.IsAny<DocumentScanQueueItem>()))
                .Callback<DocumentScanQueueItem>(item => capturedItem = item);

            var command = new UpdateDocumentCommand(documentId, "Doc", "Desc", 1, mockFile);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsError);
            Assert.NotNull(capturedItem);
            capturedItem!.FileStream.Position = 0;
            using var reader = new StreamReader(capturedItem.FileStream);
            var content = await reader.ReadToEndAsync();
            Assert.Equal("hello world!", content);
        }
    }
}
