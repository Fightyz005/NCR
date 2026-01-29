using Microsoft.Data.SqlClient;
using NCRManagementSystem.Data;
using NCRManagementSystem.Models.Entities;
using NCRManagementSystem.Repositories.Interfaces;
using System.Data;

namespace NCRManagementSystem.Repositories.Implementations
{
    public class NCRHistoryRepository : INCRHistoryRepository
    {
        private readonly DbConnection _dbConnection;

        public NCRHistoryRepository(DbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<List<NCRHistory>> GetByNCRIdAsync(int ncrId)
        {
            var history = new List<NCRHistory>();
            using var connection = await _dbConnection.CreateConnectionAsync();
            var sql = @"
                SELECT h.*, u.FullName AS ActionByName
                FROM NCRHistory h
                INNER JOIN Users u ON h.ActionBy = u.UserId
                WHERE h.NCRId = @NCRId
                ORDER BY h.ActionDate DESC";

            using var command = _dbConnection.CreateCommand(sql, connection,
                _dbConnection.CreateParameter("@NCRId", ncrId));

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                history.Add(MapNCRHistory(reader));
            }
            return history;
        }

        public async Task<int> CreateAsync(NCRHistory history)
        {
            using var connection = await _dbConnection.CreateConnectionAsync();
            var sql = @"
                INSERT INTO NCRHistory (NCRId, Action, Description, OldStatus, NewStatus, ActionDate, ActionBy, Comments)
                VALUES (@NCRId, @Action, @Description, @OldStatus, @NewStatus, @ActionDate, @ActionBy, @Comments);
                SELECT SCOPE_IDENTITY();";

            using var command = _dbConnection.CreateCommand(sql, connection,
                _dbConnection.CreateParameter("@NCRId", history.NCRId),
                _dbConnection.CreateParameter("@Action", history.Action),
                _dbConnection.CreateParameter("@Description", history.Description),
                _dbConnection.CreateParameter("@OldStatus", history.OldStatus),
                _dbConnection.CreateParameter("@NewStatus", history.NewStatus),
                _dbConnection.CreateParameter("@ActionDate", history.ActionDate),
                _dbConnection.CreateParameter("@ActionBy", history.ActionBy),
                _dbConnection.CreateParameter("@Comments", history.Comments));

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        private static NCRHistory MapNCRHistory(SqlDataReader reader)
        {
            return new NCRHistory
            {
                HistoryId = reader.GetInt32("HistoryId"),
                NCRId = reader.GetInt32("NCRId"),
                Action = reader.GetString("Action"),
                Description = reader.GetString("Description"),
                OldStatus = reader.IsDBNull("OldStatus") ? null : reader.GetString("OldStatus"),
                NewStatus = reader.IsDBNull("NewStatus") ? null : reader.GetString("NewStatus"),
                ActionDate = reader.GetDateTime("ActionDate"),
                ActionBy = reader.GetInt32("ActionBy"),
                Comments = reader.IsDBNull("Comments") ? null : reader.GetString("Comments")
            };
        }
    }
}
