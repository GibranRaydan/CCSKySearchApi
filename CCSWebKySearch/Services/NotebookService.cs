using Dapper;
using CCSWebKySearch.Models;
using System.Data;
using MySqlConnector;
using CCSWebKySearch.Exceptions;

namespace CCSWebKySearch.Services
{
    public class NotebookService : INotebookService
    {
        private readonly string _connectionString;

        public NotebookService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<IEnumerable<NotebookModel>> GetAllNotebooksAsync(int count = 500)
        {

            if (count <= 0 || count > 10000) {
                throw new InvalidInputException("Count must be between 0 and 10000.");
            }
            using (IDbConnection dbConnection = new MySqlConnection(_connectionString))
            {
                const string storedProcedure = "CCSGetDailyNotebook";
                var parameters = new DynamicParameters();
                parameters.Add("inputCount", count, DbType.Int32, ParameterDirection.Input);
                
                return await dbConnection.QueryAsync<NotebookModel>(storedProcedure, parameters, commandType: CommandType.StoredProcedure);

            } 
        }
    }

}