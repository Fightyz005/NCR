using Microsoft.Data.SqlClient;
using NCRManagementSystem.Models.DTOs;
using NCRManagementSystem.Repositories.Interfaces;
using System.Data;

namespace NCRManagementSystem.Repositories.Implementations
{
    public class ExternalPrRepository : IExternalPrRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<ExternalPrRepository> _logger;

        public ExternalPrRepository(IConfiguration configuration, ILogger<ExternalPrRepository> logger)
        {
            _connectionString = configuration.GetConnectionString("DwKpiConnection")
                ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger;
        }

        public async Task<List<ExternalPrItemDto>> GetPrItemsAsync(string banfn)
        {
            var items = new List<ExternalPrItemDto>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var sql = "SELECT * FROM [DW_KPI].[dbo].[ZPC_PR_DW] WHERE BANFN = @Banfn";
                
                using var command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@Banfn", banfn);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    items.Add(new ExternalPrItemDto
                    {
                        Banfn = reader["BANFN"].ToString() ?? string.Empty,
                        Bnfpo = reader["BNFPO"].ToString() ?? string.Empty,
                        Txz01 = reader["TXZ01"].ToString() ?? string.Empty,
                        Matnr = reader["MATNR"].ToString() ?? string.Empty,
                        Menge = reader["MENGE"] != DBNull.Value ? Convert.ToDecimal(reader["MENGE"]) : 0,
                        Meins = reader["MEINS"].ToString() ?? string.Empty,
                        Werks = reader["WERKS"].ToString() ?? string.Empty
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching external PR items for BANFN: {Banfn}", banfn);
                throw; // Rethrow to let the service/controller handle it
            }

            return items;
        }
    }
}
