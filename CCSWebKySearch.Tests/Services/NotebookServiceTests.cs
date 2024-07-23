using CCSWebKySearch.Exceptions;
using CCSWebKySearch.Models;
using CCSWebKySearch.Services;
using Dapper;
using Microsoft.Extensions.Configuration;
using Moq;
using MySqlConnector;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

public class NotebookServiceTests
{
    private readonly NotebookService _notebookService;
    private readonly Mock<IDbConnectionFactory> _mockConnectionFactory;
    private readonly string _mockConnectionString = "Server=myServerAddress;Database=myDataBase;Uid=myUsername;Pwd=myPassword;";

    public NotebookServiceTests()
    {
        _mockConnectionFactory = new Mock<IDbConnectionFactory>();
        _notebookService = new NotebookService(_mockConnectionString, _mockConnectionFactory.Object);
    }

    [Fact]
    public async Task GetAllNotebooksAsync_ShouldReturnNotebooks_WhenCountIsValid()
    {
        // Arrange
        var expectedCount = 10;
        var notebooks = new List<NotebookModel>
        {
            new NotebookModel { /* Initialize properties */ },
            new NotebookModel { /* Initialize properties */ }
        };

        var mockDbConnection = new Mock<IDbConnection>();
        mockDbConnection.Setup(db => db.QueryAsync<NotebookModel>(
            "CCSGetDailyNotebook",
            It.IsAny<DynamicParameters>(),
            null,
            null,
            CommandType.StoredProcedure))
            .ReturnsAsync(notebooks);

        _mockConnectionFactory.Setup(factory => factory.CreateConnection(It.IsAny<string>()))
                             .Returns(mockDbConnection.Object);

        // Act
        var result = await _notebookService.GetAllNotebooksAsync(expectedCount);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(notebooks.Count, result.Count());
        Assert.Equal(notebooks, result);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(10001)]
    public async Task GetAllNotebooksAsync_ShouldThrowInvalidInputException_WhenCountIsInvalid(int invalidCount)
    {
        // Act & Assert
        await Assert.ThrowsAsync<InvalidInputException>(() => _notebookService.GetAllNotebooksAsync(invalidCount));
    }
}
