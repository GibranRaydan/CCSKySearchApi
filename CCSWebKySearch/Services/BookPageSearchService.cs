using CCSWebKySearch.Models;
using Dapper;
using MySqlConnector;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using CCSWebKySearch.Exceptions;
using System.Diagnostics.Metrics;

namespace CCSWebKySearch.Services
{
    public class BookPageSearchService : ILandSearchPageBookService
    {
        private readonly string _connectionString;

        public BookPageSearchService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<IEnumerable<NotebookModel>> SearchByPageBookService(int book = 0, int page = 0)
        {
            if (book < 0 || page < 0)
            {
                throw new InvalidInputException("invalid input, just positive numbers");
            }
            using (IDbConnection dbConnection = new MySqlConnection(_connectionString))
            {
                const string storedProcedure = "LandSearch_Standard";
                var parameters = new DynamicParameters();
                parameters.Add("source", "MIXED", DbType.String, ParameterDirection.Input);
                parameters.Add("partyType", "BOTH", DbType.String, ParameterDirection.Input);
                parameters.Add("sqlWhere", $"Book = '{book}' AND Cast(Page as unsigned) = '{page}'", DbType.String, ParameterDirection.Input);
                parameters.Add("queryType", "SEARCH", DbType.String, ParameterDirection.Input);

                return await dbConnection.QueryAsync<NotebookModel>(storedProcedure, parameters, commandType: CommandType.StoredProcedure);
            }
        }
    }
}
