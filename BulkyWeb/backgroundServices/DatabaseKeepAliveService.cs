using Microsoft.Data.SqlClient;

namespace BulkyBookWeb.backgroundServices
{
    public class DatabaseKeepAliveService:BackgroundService
    {
        private readonly ILogger<DatabaseKeepAliveService> _logger;
        private readonly string _connectionString;

        public DatabaseKeepAliveService(ILogger<DatabaseKeepAliveService> logger, string connectionString)
        {
            _logger = logger;
            _connectionString = connectionString;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("DatabaseKeepAliveService is starting.");

            stoppingToken.Register(() =>
                _logger.LogInformation("DatabaseKeepAliveService is stopping."));

            // Adjust the interval (e.g., 30 minutes) based on your requirements
            TimeSpan keepAliveInterval = TimeSpan.FromMinutes(30);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var connection = new SqlConnection(_connectionString))
                    {
                        await connection.OpenAsync(stoppingToken);

                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = "SELECT 1"; // Example query to keep connection alive
                            await command.ExecuteNonQueryAsync(stoppingToken);
                        }
                    }

                    _logger.LogInformation("Keep-alive request sent successfully.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending keep-alive request.");
                }

                // Wait for the specified interval before sending the next keep-alive request
                await Task.Delay(keepAliveInterval, stoppingToken);
            }
        }
    }
}
    

