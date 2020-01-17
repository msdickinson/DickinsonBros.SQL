using Dapper;
using DickinsonBros.Logger.Abstractions;
using DickinsonBros.SQL.Abstractions;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace DickinsonBros.SQL
{
    [ExcludeFromCodeCoverage]
    public class SQLService : ISQLService
    {
        internal readonly ILoggingService<SQLService> _logger;

        public SQLService(ILoggingService<SQLService> logger)
        {
            _logger = logger;
        }

        public async Task ExecuteAsync(string connectionString, string sql, object param = null, CommandType? commandType = null)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    await connection.ExecuteAsync(
                        sql,
                        param,
                        commandType: commandType);
                }
            }
            catch (Exception exception)
            {
                var methodIdentifier = $"{nameof(SQLService)}.{nameof(ExecuteAsync)}";
                _logger.LogErrorRedacted
                (
                    $"Unhandled exception {methodIdentifier}",
                    exception,
                    new Dictionary<string, object>
                    {
                        { nameof(sql), sql },
                        { nameof(param), param },
                        { nameof(commandType), commandType }
                    }
                );
                throw;
            }
        }

        public async Task<T> QueryFirstOrDefaultAsync<T>(string connectionString, string sql, object param = null, CommandType? commandType = null)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    return await connection.QueryFirstOrDefaultAsync<T>(
                        sql,
                        param,
                        commandType: commandType);
                }
            }
            catch (Exception exception)
            {
                var methodIdentifier = $"{nameof(SQLService)}.{nameof(QueryFirstOrDefaultAsync)}";
                _logger.LogErrorRedacted
                (
                    $"Unhandled exception {methodIdentifier}",
                    exception,
                    new Dictionary<string, object>
                    {
                        { nameof(sql), sql },
                        { nameof(param), param },
                        { nameof(commandType), commandType }
                    }
                );
                throw;
            }
        }

        public async Task<T> QueryFirstAsync<T>(string connectionString, string sql, object param = null, CommandType? commandType = null)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    return await connection.QueryFirstAsync<T>(
                        sql,
                        param,
                        commandType: commandType);
                }
            }
            catch (Exception exception)
            {
                var methodIdentifier = $"{nameof(SQLService)}.{nameof(QueryFirstAsync)}";
                _logger.LogErrorRedacted
                (
                    $"Unhandled exception {methodIdentifier}",
                    exception,
                    new Dictionary<string, object>
                    {
                        { nameof(sql), sql },
                        { nameof(param), param },
                        { nameof(commandType), commandType }
                    }
                );
                throw;
            }
        }

    }
}
