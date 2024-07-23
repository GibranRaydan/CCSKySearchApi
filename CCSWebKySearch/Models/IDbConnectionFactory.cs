using MySqlConnector;
using System.Data;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection(string connectionString);
}

public class DbConnectionFactory : IDbConnectionFactory
{
    public IDbConnection CreateConnection(string connectionString)
    {
        return new MySqlConnection(connectionString);
    }
}
