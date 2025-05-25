namespace User.Api.Test.Unit
{
    public class ValidateUnitTests
    {
        [Fact]
        public void Validate()
        {
            // Arrange
            var x = 1;
            var y = 2;

            // Act
            var z = 3;

            //Assert
            Assert.Equal(x + y, z);
        }
    }
}