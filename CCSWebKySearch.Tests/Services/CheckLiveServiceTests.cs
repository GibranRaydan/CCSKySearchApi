using CCSWebKySearch.Services;
using Xunit;

namespace CCSWebKySearch.Tests.Services
{
    public class CheckLiveServiceTests
    {
        [Fact]
        public void IsLive_ReturnsTrue()
        {
            // Arrange
            var service = new CheckLiveService();

            // Act
            var result = service.IsLive();

            // Assert
            Assert.True(result);
        }
    }
}