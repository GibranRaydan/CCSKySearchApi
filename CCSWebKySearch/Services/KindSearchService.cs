using CCSWebKySearch.Models;
using Dapper;
using MySqlConnector;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using CCSWebKySearch.Exceptions;

namespace CCSWebKySearch.Services
{
    public class KindSearchService : IKindSearchService
    {
        private readonly string _connectionString;

        public KindSearchService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<IEnumerable<NotebookModel>> SearchByKindsAsync(List<string> kinds)
        {
            if (kinds == null || kinds.Count == 0)
            {
                throw new InvalidInputException("Invalid input, kinds list should not be empty");
            }

            using (IDbConnection dbConnection = new MySqlConnection(_connectionString))
            {
                const string storedProcedure = "LandSearch_Standard";
                var kindsFilter = string.Join(", ", kinds.Select(kind => $"'{kind}'"));
                var parameters = new DynamicParameters();

                parameters.Add("source", "MIXED", DbType.String, ParameterDirection.Input);
                parameters.Add("partyType", "BOTH", DbType.String, ParameterDirection.Input);
                parameters.Add("sqlWhere", $"Kind IN ({kindsFilter})", DbType.String, ParameterDirection.Input);
                parameters.Add("queryType", "SEARCH", DbType.String, ParameterDirection.Input);

                return await dbConnection.QueryAsync<NotebookModel>(storedProcedure, parameters, commandType: CommandType.StoredProcedure);
            }
        }
    }
}
