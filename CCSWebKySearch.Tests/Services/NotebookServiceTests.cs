using CCSWebKySearch.Models;
using CCSWebKySearch.Services;
using Dapper;
using Moq;
using MySqlConnector;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Configuration;

namespace CCSWebKySearch.Tests.Services
{
    public class NotebookServiceTests
    {
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<IDbConnection> _mockDbConnection;

        public NotebookServiceTests()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            _mockDbConnection = new Mock<IDbConnection>();

            _mockConfiguration.Setup(c => c.GetConnectionString("DefaultConnection"))
                .Returns("test");
        }

        [Fact]
        public async Task GetAllNotebooksAsync_ReturnsNotebooks()
        {
            // Arrange
            var mockDbConnection = new Mock<IDbConnection>();
            var notebooks = new List<NotebookModel>
            {
                new NotebookModel { Book = "1", Page = "Notebook 1" },
                new NotebookModel { Book = "2", Page = "Notebook 2" }
            };

            mockDbConnection
                .Setup(db => db.QueryAsync<NotebookModel>("CCSGetDailyNotebook", null, null, null, CommandType.StoredProcedure))
                .ReturnsAsync(notebooks);

            var service = new NotebookService(_mockConfiguration.Object);

            // Act
            var result = await service.GetAllNotebooksAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }
    }
}