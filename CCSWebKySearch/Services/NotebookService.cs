using Dapper;
using CCSWebKySearch.Models;
using System.Data;
using MySqlConnector;

namespace CCSWebKySearch.Services
{
    public class NotebookService : INotebookService
    {
        private readonly string _connectionString;

        public NotebookService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<IEnumerable<NotebookModel>> GetAllNotebooksAsync()
        {
            using (IDbConnection dbConnection = new MySqlConnection(_connectionString))
            {
                const string storedProcedure = "CCSGetDailyNotebook";
                return await dbConnection.QueryAsync<NotebookModel>(storedProcedure, commandType: CommandType.StoredProcedure);
            }
        }
    }
}
