using FunctionApp.Helper;
using FunctionApp.Models.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace FunctionApp.Models
{
    public class AppDbContext : DbContext
    {
        private readonly ILogger<AppDbContext> _logger;
        private readonly SqlHelper _sqlHelper;

        public AppDbContext(DbContextOptions<AppDbContext> options, ILogger<AppDbContext> logger, SqlHelper sqlHelper)
            : base(options)
        {
            _logger = logger;
            _sqlHelper = sqlHelper;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Grade>(entity =>
            {
                entity.ToTable("Grade"); // Ensure the table name matches your DbSet property name
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                // Add any other configuration for the Grade entity
            });

            // Add configurations for other entities if needed
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.EnableSensitiveDataLogging();
        }

        public async Task InitializeConnectionAsync()
        {
            try
            {
                _logger.LogInformation("Initializing database connection");
                var accessToken = await _sqlHelper.GetAzureSqlAccessTokenAsync();
                var conn = Database.GetDbConnection() as Microsoft.Data.SqlClient.SqlConnection;
                if (conn != null)
                {
                    Database.SetCommandTimeout(600);
                    conn.AccessToken = accessToken;
                    _logger.LogInformation("Database connection initialized successfully");
                }
                else
                {
                    _logger.LogError("Failed to get SqlConnection from DbContext");
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"Failed to initialize database connection: {ex}");
                throw;
            }
        }

        // DbSet properties
        public virtual DbSet<Grade> Grade { get; set; }
    }
}