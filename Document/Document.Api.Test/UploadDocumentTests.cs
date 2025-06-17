//using Document.Api.Common.Interfaces;
//using Document.Api.Domain.Events;
//using Document.Api.Features.Documents;
//using Document.Api.Infrastructure.Background.Interfaces;
//using Microsoft.AspNetCore.Http;
//using Moq;
//using System.Text;

//namespace Document.Api.Test
//{
//    public class UploadDocumentCommandHandlerTest
//    {
//        private readonly Mock<IDocumentScanQueue> _scanQueueMock;
//        private readonly Mock<ICurrentUserService> _userServiceMock;
//        private readonly UploadDocumentCommandHandler _handler;

//        public UploadDocumentCommandHandlerTest()
//        {
//            _scanQueueMock = new();
//            _userServiceMock = new();
//            _userServiceMock.Setup(u => u.UserId).Returns(Guid.Parse("5ae4677f-0d15-4572-ae18-597c1399f185"));

//            _handler = new UploadDocumentCommandHandler(_scanQueueMock.Object, _userServiceMock.Object);
//        }

//        private static IFormFile CreateFakeFile(string fileName = "uploaded_file.pdf", string content = "Fake file content")
//        {
//            var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
//            return new FormFile(stream, 0, stream.Length, "file", fileName)
//            {
//                Headers = new HeaderDictionary(),
//                ContentType = "application/pdf"
//            };
//        }

//        [Fact]
//        public async Task Handle_ShouldReturnGuid_WhenQueuingSucceeds()
//        {
//            // Arrange
//            var file = CreateFakeFile();
//            var command = new UploadDocumentCommand("Test Name", "Test Description", 1, file);

//            _scanQueueMock
//                .Setup(q => q.AddDocument(It.IsAny<DocumentUploadedEvent>()))
//                .ReturnsAsync(true);

//            // Act
//            var result = await _handler.Handle(command, CancellationToken.None);

//            // Assert
//            Assert.False(result.IsError);
//            Assert.IsType<Guid>(result.Value);
//            Assert.NotEqual(Guid.Empty, result.Value);
//        }

//        [Fact]
//        public async Task Handle_ShouldReturnError_WhenQueuingFails()
//        {
//            // Arrange
//            var file = CreateFakeFile();
//            var command = new UploadDocumentCommand("Test Name", "Test Description", 1, file);

//            _scanQueueMock
//                .Setup(q => q.AddDocument(It.IsAny<DocumentUploadedEvent>()))
//                .ReturnsAsync(false);

//            // Act
//            var result = await _handler.Handle(command, CancellationToken.None);

//            // Assert
//            Assert.True(result.IsError);
//            Assert.Equal(Guid.Empty, result.Value);
//            Assert.Contains(result.Errors, e => e.Description == "something went wrong trying so save the file.");
//        }

//        [Fact]
//        public async Task Handle_ShouldQueueEvent_WithCorrectData()
//        {
//            // Arrange
//            var file = CreateFakeFile("custom_file.docx", "test content");
//            var command = new UploadDocumentCommand("Doc Title", "Doc Desc", 1, file);

//            DocumentUploadedEvent? capturedEvent = null;

//            _scanQueueMock
//                .Setup(q => q.AddDocument(It.IsAny<DocumentUploadedEvent>()))
//                .Callback<DocumentUploadedEvent>(e => capturedEvent = e)
//                .ReturnsAsync(true);

//            // Act
//            var result = await _handler.Handle(command, CancellationToken.None);

//            // Assert
//            Assert.NotNull(capturedEvent);
//            Assert.Equal("Doc Title", capturedEvent.DocumentName);
//            Assert.Equal("Doc Desc", capturedEvent.DocumentDescription);
//            Assert.Equal(1, capturedEvent.Version);
//            Assert.Equal(Guid.Parse("5ae4677f-0d15-4572-ae18-597c1399f185"), capturedEvent.UploadedByUserId);
//            Assert.NotEqual(Guid.Empty, capturedEvent.DocumentId);
//        }
//    }
//}
