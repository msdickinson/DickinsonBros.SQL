using Dapper;
using DickinsonBros.Logger.Abstractions;
using DickinsonBros.SQL.Abstractions;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace DickinsonBros.SQL
{
    [ExcludeFromCodeCoverage]
    public class SQLService : ISQLService
    {
        internal readonly ILoggingService<SQLService> _logger;
        internal readonly TimeSpan DefaultBulkCopyTimeout = TimeSpan.FromMinutes(5);
        internal readonly int DefaultBatchSize = 10000;

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
                    await connection.OpenAsync().ConfigureAwait(false);

                    await connection.ExecuteAsync(
                        sql,
                        param,
                        commandType: commandType).ConfigureAwait(false); ;
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
                    await connection.OpenAsync().ConfigureAwait(false);

                    return await connection.QueryFirstOrDefaultAsync<T>(
                        sql,
                        param,
                        commandType: commandType).ConfigureAwait(false);
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
                    await connection.OpenAsync().ConfigureAwait(false);

                    return await connection.QueryFirstAsync<T>(
                        sql,
                        param,
                        commandType: commandType).ConfigureAwait(false);
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

        public async Task<IEnumerable<T>> QueryAsync<T>(string connectionString, string sql, object param = null, CommandType? commandType = null)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync().ConfigureAwait(false);

                    return await connection.QueryAsync<T>(
                        sql,
                        param,
                        commandType: commandType).ConfigureAwait(false);
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

        public async Task BulkCopyAsync<T>(string connectionString, DataTable table, string tableName, int? batchSize, TimeSpan? timeout, CancellationToken? token)
        {
            if (table == null)
            {
                throw new ArgumentNullException(nameof(table));
            }
            if (table.Rows.Count == 0)
            {
                return;
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync(token ?? CancellationToken.None).ConfigureAwait(false);

                using
                (
                    SqlBulkCopy bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, null)
                    {
                        DestinationTableName = tableName,
                        BulkCopyTimeout = (int)(timeout ?? DefaultBulkCopyTimeout).TotalSeconds,
                        BatchSize = batchSize ?? DefaultBatchSize
                    }
                )
                {
                    for (int columnIndex = 0; columnIndex < table.Columns.Count; columnIndex++)
                    {
                        DataColumn dataColumn = table.Columns[columnIndex];
                        bulkCopy.ColumnMappings.Add(dataColumn.ColumnName, dataColumn.ColumnName);
                    }
                    await bulkCopy.WriteToServerAsync(table, token ?? CancellationToken.None).ConfigureAwait(false);
                }
            }
        }
    }

}
