using Microsoft.Data.SqlClient;
using NCRManagementSystem.Data;
using NCRManagementSystem.Models.Entities;
using NCRManagementSystem.Repositories.Interfaces;
using System.Data;

namespace NCRManagementSystem.Repositories.Implementations
{
    public class NCRCommentRepository : INCRCommentRepository
    {
        private readonly DbConnection _dbConnection;

        public NCRCommentRepository(DbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<List<NCRComment>> GetByNCRIdAsync(int ncrId)
        {
            var comments = new List<NCRComment>();
            using var connection = await _dbConnection.CreateConnectionAsync();
            var sql = @"
                SELECT c.*, u.FullName AS CreatedByName
                FROM NCRComments c
                INNER JOIN Users u ON c.CreatedBy = u.UserId
                WHERE c.NCRId = @NCRId
                ORDER BY c.CreatedDate ASC";

            using var command = _dbConnection.CreateCommand(sql, connection,
                _dbConnection.CreateParameter("@NCRId", ncrId));

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                comments.Add(MapNCRComment(reader));
            }
            return comments;
        }

        public async Task<int> CreateAsync(NCRComment comment)
        {
            using var connection = await _dbConnection.CreateConnectionAsync();
            var sql = @"
                INSERT INTO NCRComments (NCRId, CommentText, CommentType, CreatedDate, CreatedBy, IsResolved, ParentCommentId)
                VALUES (@NCRId, @CommentText, @CommentType, @CreatedDate, @CreatedBy, @IsResolved, @ParentCommentId);
                SELECT SCOPE_IDENTITY();";

            using var command = _dbConnection.CreateCommand(sql, connection,
                _dbConnection.CreateParameter("@NCRId", comment.NCRId),
                _dbConnection.CreateParameter("@CommentText", comment.CommentText),
                _dbConnection.CreateParameter("@CommentType", comment.CommentType),
                _dbConnection.CreateParameter("@CreatedDate", comment.CreatedDate),
                _dbConnection.CreateParameter("@CreatedBy", comment.CreatedBy),
                _dbConnection.CreateParameter("@IsResolved", comment.IsResolved),
                _dbConnection.CreateParameter("@ParentCommentId", comment.ParentCommentId));

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<bool> UpdateAsync(NCRComment comment)
        {
            using var connection = await _dbConnection.CreateConnectionAsync();
            var sql = @"
                UPDATE NCRComments 
                SET CommentText = @CommentText, 
                    CommentType = @CommentType, 
                    IsResolved = @IsResolved
                WHERE CommentId = @CommentId";

            using var command = _dbConnection.CreateCommand(sql, connection,
                _dbConnection.CreateParameter("@CommentId", comment.CommentId),
                _dbConnection.CreateParameter("@CommentText", comment.CommentText),
                _dbConnection.CreateParameter("@CommentType", comment.CommentType),
                _dbConnection.CreateParameter("@IsResolved", comment.IsResolved));

            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(int commentId)
        {
            using var connection = await _dbConnection.CreateConnectionAsync();
            using var transaction = connection.BeginTransaction();

            try
            {
                // First delete any replies to this comment
                var deleteReplies = "DELETE FROM NCRComments WHERE ParentCommentId = @CommentId";
                using var repliesCmd = _dbConnection.CreateCommand(deleteReplies, connection,
                    _dbConnection.CreateParameter("@CommentId", commentId));
                repliesCmd.Transaction = transaction;
                await repliesCmd.ExecuteNonQueryAsync();

                // Then delete the comment itself
                var deleteComment = "DELETE FROM NCRComments WHERE CommentId = @CommentId";
                using var commentCmd = _dbConnection.CreateCommand(deleteComment, connection,
                    _dbConnection.CreateParameter("@CommentId", commentId));
                commentCmd.Transaction = transaction;
                var rowsAffected = await commentCmd.ExecuteNonQueryAsync();

                transaction.Commit();
                return rowsAffected > 0;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<NCRComment?> GetByIdAsync(int commentId)
        {
            using var connection = await _dbConnection.CreateConnectionAsync();
            var sql = @"
                SELECT c.*, u.FullName AS CreatedByName
                FROM NCRComments c
                INNER JOIN Users u ON c.CreatedBy = u.UserId
                WHERE c.CommentId = @CommentId";

            using var command = _dbConnection.CreateCommand(sql, connection,
                _dbConnection.CreateParameter("@CommentId", commentId));

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapNCRComment(reader);
            }
            return null;
        }

        public async Task<List<NCRComment>> GetRepliesAsync(int parentCommentId)
        {
            var replies = new List<NCRComment>();
            using var connection = await _dbConnection.CreateConnectionAsync();
            var sql = @"
                SELECT c.*, u.FullName AS CreatedByName
                FROM NCRComments c
                INNER JOIN Users u ON c.CreatedBy = u.UserId
                WHERE c.ParentCommentId = @ParentCommentId
                ORDER BY c.CreatedDate ASC";

            using var command = _dbConnection.CreateCommand(sql, connection,
                _dbConnection.CreateParameter("@ParentCommentId", parentCommentId));

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                replies.Add(MapNCRComment(reader));
            }
            return replies;
        }

        public async Task<bool> MarkAsResolvedAsync(int commentId, bool isResolved)
        {
            using var connection = await _dbConnection.CreateConnectionAsync();
            var sql = "UPDATE NCRComments SET IsResolved = @IsResolved WHERE CommentId = @CommentId";

            using var command = _dbConnection.CreateCommand(sql, connection,
                _dbConnection.CreateParameter("@CommentId", commentId),
                _dbConnection.CreateParameter("@IsResolved", isResolved));

            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<int> GetCommentCountByNCRIdAsync(int ncrId)
        {
            using var connection = await _dbConnection.CreateConnectionAsync();
            var sql = "SELECT COUNT(*) FROM NCRComments WHERE NCRId = @NCRId";

            using var command = _dbConnection.CreateCommand(sql, connection,
                _dbConnection.CreateParameter("@NCRId", ncrId));

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<List<NCRComment>> GetCommentsByTypeAsync(int ncrId, string commentType)
        {
            var comments = new List<NCRComment>();
            using var connection = await _dbConnection.CreateConnectionAsync();
            var sql = @"
                SELECT c.*, u.FullName AS CreatedByName
                FROM NCRComments c
                INNER JOIN Users u ON c.CreatedBy = u.UserId
                WHERE c.NCRId = @NCRId AND c.CommentType = @CommentType
                ORDER BY c.CreatedDate ASC";

            using var command = _dbConnection.CreateCommand(sql, connection,
                _dbConnection.CreateParameter("@NCRId", ncrId),
                _dbConnection.CreateParameter("@CommentType", commentType));

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                comments.Add(MapNCRComment(reader));
            }
            return comments;
        }

        public async Task<List<NCRComment>> GetUnresolvedCommentsAsync(int ncrId)
        {
            var comments = new List<NCRComment>();
            using var connection = await _dbConnection.CreateConnectionAsync();
            var sql = @"
                SELECT c.*, u.FullName AS CreatedByName
                FROM NCRComments c
                INNER JOIN Users u ON c.CreatedBy = u.UserId
                WHERE c.NCRId = @NCRId AND c.IsResolved = 0
                ORDER BY c.CreatedDate ASC";

            using var command = _dbConnection.CreateCommand(sql, connection,
                _dbConnection.CreateParameter("@NCRId", ncrId));

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                comments.Add(MapNCRComment(reader));
            }
            return comments;
        }

        public async Task<bool> BulkDeleteByNCRIdAsync(int ncrId)
        {
            using var connection = await _dbConnection.CreateConnectionAsync();
            var sql = "DELETE FROM NCRComments WHERE NCRId = @NCRId";

            using var command = _dbConnection.CreateCommand(sql, connection,
                _dbConnection.CreateParameter("@NCRId", ncrId));

            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        private static NCRComment MapNCRComment(SqlDataReader reader)
        {
            return new NCRComment
            {
                CommentId = reader.GetInt32("CommentId"),
                NCRId = reader.GetInt32("NCRId"),
                CommentText = reader.GetString("CommentText"),
                CommentType = reader.GetString("CommentType"),
                CreatedDate = reader.GetDateTime("CreatedDate"),
                CreatedBy = reader.GetInt32("CreatedBy"),
                IsResolved = reader.GetBoolean("IsResolved"),
                ParentCommentId = reader.IsDBNull("ParentCommentId") ? null : reader.GetInt32("ParentCommentId"),
                // Navigation properties would be loaded separately if needed
                CreatedByUser = new User
                {
                    UserId = reader.GetInt32("CreatedBy"),
                    FullName = reader.GetString("CreatedByName")
                }
            };
        }
    }
}