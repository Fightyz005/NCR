using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using NCRManagementSystem.Configuration;
using System.Data;

namespace NCRManagementSystem.Data
{
    public class DbConnection
    {
        private readonly string _connectionString;
        private readonly DatabaseSettings _databaseSettings;

        public DbConnection(IConfiguration configuration, IOptions<DatabaseSettings> databaseSettings)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException(nameof(configuration));
            _databaseSettings = databaseSettings.Value;
        }

        public SqlConnection CreateConnection()
        {
            var connection = new SqlConnection(_connectionString);
            return connection;
        }

        public async Task<SqlConnection> CreateConnectionAsync()
        {
            var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            return connection;
        }

        public SqlCommand CreateCommand(string sql, SqlConnection connection, params SqlParameter[] parameters)
        {
            var command = new SqlCommand(sql, connection)
            {
                CommandTimeout = _databaseSettings.CommandTimeout
            };

            if (parameters != null)
            {
                command.Parameters.AddRange(parameters);
            }

            return command;
        }

        public SqlParameter CreateParameter(string name, object? value)
        {
            return new SqlParameter(name, value ?? DBNull.Value);
        }

        public SqlParameter CreateParameter(string name, SqlDbType type, object? value)
        {
            return new SqlParameter(name, type) { Value = value ?? DBNull.Value };
        }
    }
}
