using CCSWebKySearch.Models;
using Dapper;
using MySqlConnector;
using System.Data;
using CCSWebKySearch.Exceptions;
using CCSWebKySearch.Contracts;

namespace CCSWebKySearch.Services
{
    public class MarriageLicenseSearchService : IMarriageLicenseService
    {
        private readonly string _connectionString;

        public MarriageLicenseSearchService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<IEnumerable<MarriageLicenseModel>> SearchMarriageLicense(
            string surname, 
            string searchType, 
            int order = 0)
        {
            string sortDirection = _inputValidations(surname, searchType, order);
            using (IDbConnection dbConnection = new MySqlConnection(_connectionString))
            {
                string storedProcedure = "";
                var parameters = new DynamicParameters();

                if (searchType == "GROOM") 
                {
                    storedProcedure = "VitalSearch_MarriageGroom";
                    parameters.Add(
                        "sqlWhere", $" WHERE Groomsurname like '{surname}%'",
                        DbType.String, 
                        ParameterDirection.Input
                        );
                }
                if (searchType == "BRIDE")
                {
                    storedProcedure = "VitalSearch_MarriageBride";
                    parameters.Add(
                        "sqlWhere", $" WHERE bridesurname like '{surname}%'",
                        DbType.String,
                        ParameterDirection.Input
                        );
                }
                parameters.Add("searchLocation", "MIXED", DbType.String, ParameterDirection.Input);
                parameters.Add("SortDirection", sortDirection, DbType.String, ParameterDirection.Input);
                parameters.Add("OnlyDistinctDocs", 0, DbType.Int16, ParameterDirection.Input);

                return await dbConnection.QueryAsync<MarriageLicenseModel>(storedProcedure, parameters, commandType: CommandType.StoredProcedure);
            }
        }

        private static string _inputValidations(string surname, string searchType, int order)
        {
            if (string.IsNullOrEmpty(surname))
            {
                throw new InvalidInputException("invalid input, surname can not be empty");
            }

            if (searchType != "GROOM" && searchType != "BRIDE")
            {
                throw new InvalidInputException("invalid input searchType should be GROOM or BRIDE");
            }

            var sortDirection = "";

            switch (order)
            {
                case 0:
                    sortDirection = "asc";
                    break;

                case 1: 
                    sortDirection = "desc";
                    break;
                default:
                    throw new InvalidInputException("invalid input count should be 0 or 1"); ;
            }

            return sortDirection;
        }
    }
}
