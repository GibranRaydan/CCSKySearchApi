using CCSWebKySearch.Models;
using Dapper;
using MySqlConnector;
using System.Data;
using CCSWebKySearch.Exceptions;
using CCSWebKySearch.Contracts;

namespace CCSWebKySearch.Services
{
    public class KindSearchService : IKindSearchService
    {
        private readonly string _connectionString;
        private readonly HashSet<string> _validDocumentTypes = new HashSet<string>
        {
            "0", "AOS", "AOC", "AFDT", "AGR", "SUBA", "DEEDA", "EASMA", "ENCA",
            "COVAM", "AC", "ASGN", "RENT", "ASRNT", "AN", "ASUAG", "AVOID", "BB",
            "BBR", "MLBR", "CL", "CLA", "CLAS", "CLPR", "CLR", "CT", "CONT", "CAFDT",
            "CAC", "CASGN", "CCOV", "LEASC", "CREL", "DEED", "DEEDN", "DAFDT", "DOC",
            "EASM", "REASM", "ENC", "ENCAS", "ENCPR", "ENCR", "FLIEN", "FLR", "FS",
            "FSN", "FIX", "FFAM", "FFA", "FFC", "FFPR", "FFR", "GUARD", "LR", "LD",
            "LEASE", "LEASN", "LEASA", "LEASEA", "LEASER", "DEEDM", "MMTG", "MLR",
            "MEC", "MLA", "MIS", "MODAG", "MTG", "MTGN", "MTGAG", "MTGAM", "MTGC",
            "MTGM", "MTGWA", "NC", "NB", "NOTARY BON", "NBR", "OPT", "ORDER", "ORD",
            "PREL", "PL", "PA", "COV", "REMTG", "REL", "RMIS", "RPOA", "SLIEN", "SLPR",
            "SLR", "SUB", "TLIEN", "TLA", "TREL", "TLSAM", "TLSA", "TLSPR", "WILL",
            "WILLC", "WD", "WILLN", "WILLR"
        };

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

            List<string> validKinds;

            if (kinds.Contains("0"))
            {
                validKinds = _validDocumentTypes.ToList();
            }
            else
            {
                validKinds = kinds.Where(kind => _validDocumentTypes.Contains(kind)).ToList();
            }

            if (validKinds.Count == 0)
            {
                throw new InvalidInputException("No valid kinds provided");
            }

            using (IDbConnection dbConnection = new MySqlConnection(_connectionString))
            {
                const string storedProcedure = "LandSearch_Standard";
                var kindsFilter = string.Join(", ", validKinds.Select(kind => $"'{kind}'"));
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
