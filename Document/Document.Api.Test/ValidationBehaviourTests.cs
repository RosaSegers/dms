using Document.Api.Common.Behaviour;
using ErrorOr;
using FluentValidation;
using MediatR;
using Moq;

namespace Document.Api.Test
{
    public class ValidationBehaviourTests
    {
        [Fact]
        public async Task Handle_Should_Call_Next_When_No_Validator_Is_Provided()
        {
            // Arrange
            var mockRequest = new MyRequest();
            var mockResponse = new MyResponse();

            // Create a mock of the RequestHandlerDelegate that takes a CancellationToken
            var nextDelegate = new Mock<RequestHandlerDelegate<ErrorOr<MyResponse>>>();

            // Setup the delegate to return a Task with mockResponse when invoked
            nextDelegate.Setup(nd => nd(It.IsAny<CancellationToken>())).ReturnsAsync(mockResponse);

            var validationBehaviour = new ValidationBehaviour<MyRequest, ErrorOr<MyResponse>>();

            // Act
            var result = await validationBehaviour.Handle(mockRequest, nextDelegate.Object, CancellationToken.None);

            // Assert
            nextDelegate.Verify(nd => nd(It.IsAny<CancellationToken>()), Times.Once);  // Ensure next() was called exactly once
        }

        [Fact]
        public async Task Handle_Should_Call_Next_When_Validation_Is_Valid()
        {
            // Arrange
            var mockValidator = new Mock<IValidator<MyRequest>>();
            var mockRequest = new MyRequest();
            var mockResponse = new MyResponse();

            var validatorResult = new FluentValidation.Results.ValidationResult();
            mockValidator.Setup(v => v.ValidateAsync(It.IsAny<MyRequest>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(validatorResult);

            var validationBehaviour = new ValidationBehaviour<MyRequest, ErrorOr<MyResponse>>(mockValidator.Object);

            // Create a mock of the RequestHandlerDelegate that takes a CancellationToken
            var nextDelegate = new Mock<RequestHandlerDelegate<ErrorOr<MyResponse>>>();
            nextDelegate.Setup(nd => nd(It.IsAny<CancellationToken>())).ReturnsAsync(mockResponse);

            // Act
            var result = await validationBehaviour.Handle(mockRequest, nextDelegate.Object, CancellationToken.None);

            // Assert
            nextDelegate.Verify(nd => nd(It.IsAny<CancellationToken>()), Times.Once);  // Ensure next() was called exactly once
        }

        [Fact]
        public async Task Handle_Should_Return_Errors_When_Validation_Fails()
        {
            // Arrange
            var mockValidator = new Mock<IValidator<MyRequest>>();
            var mockRequest = new MyRequest();

            // Create validation errors
            var validationErrors = new FluentValidation.Results.ValidationResult(
                new[]
                {
            new FluentValidation.Results.ValidationFailure("Name", "Name is required")
                });

            mockValidator.Setup(v => v.ValidateAsync(It.IsAny<MyRequest>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(validationErrors);

            var validationBehaviour = new ValidationBehaviour<MyRequest, ErrorOr<MyResponse>>(mockValidator.Object);

            // Create a mock of the RequestHandlerDelegate that takes a CancellationToken
            var nextDelegate = new Mock<RequestHandlerDelegate<ErrorOr<MyResponse>>>();
            nextDelegate.Setup(nd => nd(It.IsAny<CancellationToken>())).ReturnsAsync(new ErrorOr<MyResponse>());

            // Act
            var result = await validationBehaviour.Handle(mockRequest, nextDelegate.Object, CancellationToken.None);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("Name", result.FirstError.Code);
            Assert.Equal("Name is required", result.FirstError.Description);
        }


        // Request and Response classes used for testing
        public class MyRequest : IRequest<ErrorOr<MyResponse>> { }
        public class MyResponse { }
    }
}