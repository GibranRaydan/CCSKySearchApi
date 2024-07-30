using CCSWebKySearch.Contracts;
using CCSWebKySearch.Exceptions;
using CCSWebKySearch.Models;
using Dapper;
using MySqlConnector;
using System.Data;
using System.Security.AccessControl;
using System.Text.RegularExpressions;

namespace CCSWebKySearch.Services
{

    public class NameSearchService : INameSearchService
    {
        private readonly string _connectionString;

        public NameSearchService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<IEnumerable<NotebookModel>> SearchByNameServiceAsync(string surname, string nameType = "BOTH", string given = null)
        {
            nameType = nameType.Trim();
            surname = Regex.Replace(surname, @"[^a-zA-Z0-9áéíóúÁÉÍÓÚ]", "");
            given = Regex.Replace(given, @"[^a-zA-Z0-9áéíóúÁÉÍÓÚ]", "");


            if (string.IsNullOrWhiteSpace(surname))
            {
                throw new InvalidInputException("Invalid input, surname cannot be empty");
            }

            if (nameType != "GRANTOR" && nameType != "GRANTEE" && nameType != "BOTH")
            {
                throw new InvalidInputException("Invalid input, nameType must be 'GRANTOR', 'GRANTEE', or 'BOTH'");
            }

            if (nameType == "BOTH" && String.IsNullOrEmpty(given))
            {
                throw new InvalidInputException("Invalid input, for nameType 'BOTH' the name field is required");
            }

            using (IDbConnection dbConnection = new MySqlConnection(_connectionString))
            {
                const string storedProcedure = "LandSearch_Standard";
                var parameters = new DynamicParameters();
                parameters.Add("source", "MIXED", DbType.String, ParameterDirection.Input);
                parameters.Add("partyType", nameType, DbType.String, ParameterDirection.Input);
                string sqlWhere = ConstructSqlWhereClause(nameType, surname, given);
                parameters.Add("sqlWhere", sqlWhere, DbType.String, ParameterDirection.Input);
                parameters.Add("queryType", "SEARCH", DbType.String, ParameterDirection.Input);

                return await dbConnection.QueryAsync<NotebookModel>(storedProcedure, parameters, commandType: CommandType.StoredProcedure);
            }
        }

        private string ConstructSqlWhereClause(string nameType, string surname, string given)
        {
            string baseCondition = string.Empty;

            if (nameType == "GRANTOR")
            {
                baseCondition = $"(GrantorStripped LIKE '{surname}%' AND (LEFT(Grantor, 7) <> 'NOTHING' AND NOT (LEFT(code, 1) = 'I' AND GrantorGiven1 = '')) AND IsEEFirstParty=1)";

            }

            if (nameType == "GRANTEE")
            {
                baseCondition = $"(GranteeStripped LIKE '{surname}%' AND (LEFT(Grantee, 6) <> 'NOBODY' AND NOT (LENGTH(code) > 1 AND RIGHT(code, 1) = 'I' AND GranteeGiven1 = '')) AND IsORFirstParty=1)";
            }

            if (nameType == "BOTH")
            {
                baseCondition = $"(GrantorStripped LIKE '{surname}%' AND GrantorGiven1Stripped LIKE '{given}%' AND (LEFT(Grantor, 7) <> 'NOTHING' AND NOT (LEFT(code, 1) = 'I' AND GrantorGiven1 = '')) AND IsEEFirstParty=1)";
            }


            return $" WHERE {baseCondition}";
        }
    }
}
