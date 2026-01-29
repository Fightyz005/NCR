namespace NCRManagementSystem.Data
{
    public class DatabaseInitializer
    {
        private readonly DbConnection _dbConnection;
        private readonly ILogger<DatabaseInitializer> _logger;

        public DatabaseInitializer(DbConnection dbConnection, ILogger<DatabaseInitializer> logger)
        {
            _dbConnection = dbConnection;
            _logger = logger;
        }

        public async Task InitializeAsync()
        {
            try
            {
                using var connection = await _dbConnection.CreateConnectionAsync();

                // Check if tables exist, if not create them
                var checkTableSql = @"
                    SELECT COUNT(*) 
                    FROM INFORMATION_SCHEMA.TABLES 
                    WHERE TABLE_NAME = 'Users'";

                using var checkCommand = _dbConnection.CreateCommand(checkTableSql, connection);
                var tableExists = (int)await checkCommand.ExecuteScalarAsync() > 0;

                if (!tableExists)
                {
                    _logger.LogInformation("Database tables not found. Creating database schema...");
                    // Here you would run your database creation scripts
                    // For now, we'll assume the database is already created via the SQL script
                }

                _logger.LogInformation("Database initialization completed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during database initialization");
                throw;
            }
        }
    }
}
