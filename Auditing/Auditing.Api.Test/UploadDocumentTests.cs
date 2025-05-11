using Auditing.Api.Common.Interfaces;
using Auditing.Api.Domain.Events;
using Auditing.Api.Features.Auditings;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Text;

namespace Auditing.Api.Test
{
    public class UploadAuditingQueryHandlerTest
    {
        private readonly Mock<IAuditingStorage> _storageMock;
        private readonly Mock<ICurrentUserService> _userServiceMock;
        private readonly UploadAuditingQueryHandler _handler;

        public UploadAuditingQueryHandlerTest()
        {
            _storageMock = new();
            _userServiceMock = new();
            _userServiceMock.Setup(u => u.UserId).Returns(Guid.Parse("5ae4677f-0d15-4572-ae18-597c1399f185"));

            _handler = new UploadAuditingQueryHandler(_storageMock.Object, _userServiceMock.Object);
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
        public async Task Handle_ShouldReturnGuid_WhenAddAuditingSucceeds()
        {
            // Arrange
            var file = CreateFakeFile();
            var query = new UploadAuditingQuery("Test Name", "Test Description", 1, file);

            _storageMock
                .Setup(s => s.AddAuditing(It.IsAny<AuditingUploadedEvent>()))
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsError);
            Assert.IsType<Guid>(result.Value);
            Assert.NotEqual(Guid.Empty, result.Value);
        }

        [Fact]
        public async Task Handle_ShouldReturnError_WhenAddAuditingFails()
        {
            // Arrange
            var file = CreateFakeFile();
            var query = new UploadAuditingQuery("Test Name", "Test Description", 1, file);

            _storageMock
                .Setup(s => s.AddAuditing(It.IsAny<AuditingUploadedEvent>()))
                .ReturnsAsync(false);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal(Guid.Empty, result.Value);
            Assert.Contains(result.Errors, e => e.Code == "something went wrong trying so save the file.");
        }

        [Fact]
        public async Task Handle_ShouldCall_AddAuditing_WithCorrectEventData()
        {
            // Arrange
            var file = CreateFakeFile("custom_file.docx", "some content here");
            var query = new UploadAuditingQuery("Doc Title", "Doc Desc", 1, file);
            IAuditingEvent? capturedEvent = null;

            _storageMock
                .Setup(s => s.AddAuditing(It.IsAny<AuditingUploadedEvent>()))
                .Callback<IAuditingEvent>(e => capturedEvent = e)
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(capturedEvent);
            Assert.NotEqual(Guid.Empty, capturedEvent.Id);
            Assert.Equal(1, capturedEvent.Version);
        }
    }
}
