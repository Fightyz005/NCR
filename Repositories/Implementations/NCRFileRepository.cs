using Microsoft.Data.SqlClient;
using NCRManagementSystem.Data;
using NCRManagementSystem.Models.Entities;
using NCRManagementSystem.Repositories.Interfaces;
using System.Data;

namespace NCRManagementSystem.Repositories.Implementations
{
    public class NCRFileRepository : INCRFileRepository
    {
        private readonly DbConnection _dbConnection;

        public NCRFileRepository(DbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<NCRFile?> GetByIdAsync(int fileId)
        {
            using var connection = await _dbConnection.CreateConnectionAsync();
            var sql = "SELECT * FROM NCRFiles WHERE FileId = @FileId";
            using var command = _dbConnection.CreateCommand(sql, connection,
                _dbConnection.CreateParameter("@FileId", fileId));

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapNCRFile(reader);
            }
            return null;
        }

        public async Task<List<NCRFile>> GetByNCRIdAsync(int ncrId)
        {
            var files = new List<NCRFile>();
            using var connection = await _dbConnection.CreateConnectionAsync();
            var sql = "SELECT * FROM NCRFiles WHERE NCRId = @NCRId ORDER BY UploadedDate DESC";
            using var command = _dbConnection.CreateCommand(sql, connection,
                _dbConnection.CreateParameter("@NCRId", ncrId));

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                files.Add(MapNCRFile(reader));
            }
            return files;
        }

        public async Task<int> CreateAsync(NCRFile file)
        {
            using var connection = await _dbConnection.CreateConnectionAsync();
            var sql = @"
                INSERT INTO NCRFiles (NCRId, FileName, OriginalFileName, FilePath, FileSize, FileType, 
                                     UploadedDate, UploadedBy, FileCategory)
                VALUES (@NCRId, @FileName, @OriginalFileName, @FilePath, @FileSize, @FileType,
                        @UploadedDate, @UploadedBy, @FileCategory);
                SELECT SCOPE_IDENTITY();";

            using var command = _dbConnection.CreateCommand(sql, connection,
                _dbConnection.CreateParameter("@NCRId", file.NCRId),
                _dbConnection.CreateParameter("@FileName", file.FileName),
                _dbConnection.CreateParameter("@OriginalFileName", file.OriginalFileName),
                _dbConnection.CreateParameter("@FilePath", file.FilePath),
                _dbConnection.CreateParameter("@FileSize", file.FileSize),
                _dbConnection.CreateParameter("@FileType", file.FileType),
                _dbConnection.CreateParameter("@UploadedDate", file.UploadedDate),
                _dbConnection.CreateParameter("@UploadedBy", file.UploadedBy),
                _dbConnection.CreateParameter("@FileCategory", file.FileCategory));

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<bool> DeleteAsync(int fileId)
        {
            using var connection = await _dbConnection.CreateConnectionAsync();
            var sql = "DELETE FROM NCRFiles WHERE FileId = @FileId";
            using var command = _dbConnection.CreateCommand(sql, connection,
                _dbConnection.CreateParameter("@FileId", fileId));

            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteByNCRIdAsync(int ncrId)
        {
            using var connection = await _dbConnection.CreateConnectionAsync();
            var sql = "DELETE FROM NCRFiles WHERE NCRId = @NCRId";
            using var command = _dbConnection.CreateCommand(sql, connection,
                _dbConnection.CreateParameter("@NCRId", ncrId));

            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        private static NCRFile MapNCRFile(SqlDataReader reader)
        {
            return new NCRFile
            {
                FileId = reader.GetInt32("FileId"),
                NCRId = reader.GetInt32("NCRId"),
                FileName = reader.GetString("FileName"),
                OriginalFileName = reader.GetString("OriginalFileName"),
                FilePath = reader.GetString("FilePath"),
                FileSize = reader.GetInt64("FileSize"),
                FileType = reader.GetString("FileType"),
                UploadedDate = reader.GetDateTime("UploadedDate"),
                UploadedBy = reader.GetInt32("UploadedBy"),
                FileCategory = reader.GetString("FileCategory")
            };
        }
    }
}
