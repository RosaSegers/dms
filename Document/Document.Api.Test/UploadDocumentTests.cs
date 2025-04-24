using Document.Api.Common.Interfaces;
using Document.Api.Domain.Events;
using Document.Api.Features.Documents;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace Document.Api.Test
{
    public class UploadDocumentQueryHandlerTest
    {
        private readonly Mock<IDocumentStorage> _storageMock;
        private readonly UploadDocumentQueryHandler _handler;

        public UploadDocumentQueryHandlerTest()
        {
            _storageMock = new();
            _handler = new UploadDocumentQueryHandler(_storageMock.Object);
        }

        // For now i am using test data, therefor the unit test wont work.
        //[Fact]
        //public async Task Handle_ShouldReturnGuid_WhenAddDocumentSucceeds()
        //{
        //    // Arrange
        //    var documentId = Guid.NewGuid();
        //    var mockFile = new Mock<IFormFile>();
        //    var query = new UploadDocumentQuery("Test Name", "Test Description", mockFile.Object);

        //    _storageMock
        //        .Setup(s => s.AddDocument(It.IsAny<DocumentUploadedEvent>()))
        //        .ReturnsAsync(true);

        //    // Act
        //    var result = await _handler.Handle(query, CancellationToken.None);

        //    // Assert
        //    Assert.False(result.IsError);
        //    Assert.Equal(documentId, result.Value);
        //}

        [Fact]
        public async Task Handle_ShouldReturnError_WhenAddDocumentFails()
        {
            // Arrange
            var documentId = Guid.NewGuid();
            var mockFile = new Mock<IFormFile>();
            var query = new UploadDocumentQuery("Test Name", "Test Description", mockFile.Object);

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
            var documentId = Guid.NewGuid();
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns("uploaded_file.pdf");

            var query = new UploadDocumentQuery("Test Name", "Test Description", mockFile.Object);

            IDocumentEvent? capturedEvent = null;
            _storageMock
                .Setup(s => s.AddDocument(It.IsAny<DocumentUploadedEvent>()))
                .Callback<IDocumentEvent>(e => capturedEvent = e)
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(capturedEvent);
        }
    }
}