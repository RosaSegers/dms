using Document.Api.Common.Interfaces;
using Document.Api.Domain.Events;
using Document.Api.Features.Documents;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Document.Api.Test
{
    public class RolebackDocumentTests
    {
        private readonly Mock<IDocumentStorage> _storageMock;
        private readonly Mock<ICurrentUserService> _userServiceMock;
        private readonly RolebackDocumentQueryHandler _handler;

        public RolebackDocumentTests()
        {
            _storageMock = new Mock<IDocumentStorage>();
            _userServiceMock = new Mock<ICurrentUserService>();
            _userServiceMock.Setup(u => u.UserId).Returns(Guid.NewGuid());

            _handler = new RolebackDocumentQueryHandler(_storageMock.Object, _userServiceMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnId_WhenAddDocumentSucceeds()
        {
            // Arrange
            var docId = Guid.NewGuid();
            var query = new RolebackDocumentQuery(docId, Version: 3);

            var versions = new List<IDocumentEvent>
            {
                new DocumentVersion {Version = 1},
                new DocumentVersion {Version = 2},
                new DocumentVersion {Version = 3},
                new DocumentVersion {Version = 4}
            };

            _storageMock
                .Setup(s => s.GetDocumentById(docId))
                .ReturnsAsync(versions);

            _storageMock
                .Setup(s => s.AddDocument(It.IsAny<DocumentRolebackEvent>()))
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsError);
            Assert.IsType<Guid>(result.Value);
            _storageMock.Verify(s => s.AddDocument(It.Is<DocumentRolebackEvent>(e =>
                e.DocumentId != Guid.Empty &&
                e.Version == 3
            )), Times.Once);
        }
    }

    internal class DocumentVersion : IDocumentEvent
    {
        public Guid DocumentId { get; set; }
        public float? Version { get; set; }
        public DateTime OccurredAt { get; set; }

        public string EventType => throw new NotImplementedException();

        public string Id => throw new NotImplementedException();
    }
}
