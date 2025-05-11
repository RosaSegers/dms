using Auditing.Api.Common.Interfaces;
using Auditing.Api.Domain.Events;
using Auditing.Api.Features.Auditings;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Auditing.Api.Test
{
    public class UpdateAuditingQueryHandlerTest
    {
        private readonly Mock<IAuditingStorage> _storageMock;
        private readonly Mock<ICurrentUserService> _userServiceMock;
        private readonly UpdateAuditingQueryHandler _handler;

        public UpdateAuditingQueryHandlerTest()
        {
            _storageMock = new();
            _userServiceMock = new();
            _userServiceMock.Setup(u => u.UserId).Returns(Guid.Parse("5ae4677f-0d15-4572-ae18-597c1399f185"));

            _handler = new UpdateAuditingQueryHandler(_storageMock.Object, _userServiceMock.Object);
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
            var AuditingId = Guid.NewGuid();
            var mockFile = CreateFakeFile();

            var query = new UpdateAuditingQuery(AuditingId, "Updated Name", "Updated Description", 1, mockFile);

            _storageMock
                .Setup(s => s.AddAuditing(It.IsAny<AuditingUpdatedEvent>()))
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsError);
            Assert.Equal(AuditingId, result.Value);
        }

        [Fact]
        public async Task Handle_ShouldReturnError_WhenAddAuditingFails()
        {
            // Arrange
            var AuditingId = Guid.NewGuid();
            var mockFile = CreateFakeFile();

            var query = new UpdateAuditingQuery(AuditingId, "Updated Name", "Updated Description", 1, mockFile);

            _storageMock
                .Setup(s => s.AddAuditing(It.IsAny<AuditingUpdatedEvent>()))
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
            var AuditingId = Guid.NewGuid();
            var mockFile = CreateFakeFile("custom_file.docx", "some content here");

            var query = new UpdateAuditingQuery(AuditingId, "Updated Name", "Updated Description", 1, mockFile);

            IAuditingEvent? capturedEvent = null;

            _storageMock
                .Setup(s => s.AddAuditing(It.IsAny<AuditingUpdatedEvent>()))
                .Callback<IAuditingEvent>(e => capturedEvent = e)
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(capturedEvent);
            Assert.Equal(AuditingId, capturedEvent.Id);
            Assert.NotEqual(Guid.Empty, capturedEvent.Id);
        }
    }
}
