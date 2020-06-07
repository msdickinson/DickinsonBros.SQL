using Dapper;
using DickinsonBros.DateTime.Abstractions;
using DickinsonBros.Logger.Abstractions;
using DickinsonBros.Redactor.Abstractions;
using DickinsonBros.SQL.Abstractions;
using DickinsonBros.Stopwatch.Abstractions;
using DickinsonBros.Telemetry.Abstractions;
using DickinsonBros.Telemetry.Abstractions.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
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
        internal readonly IServiceProvider _serviceProvider;
        internal readonly ILoggingService<SQLService> _logger;
        internal readonly TimeSpan DefaultBulkCopyTimeout = TimeSpan.FromMinutes(5);
        internal readonly int DefaultBatchSize = 10000;
        internal readonly ITelemetryService _telemetryService;
        internal readonly IDateTimeService _dateTimeService;
        internal readonly IRedactorService _redactorService;
        public SQLService
        (
            IServiceProvider serviceProvider,
            ILoggingService<SQLService> logger,
            IRedactorService redactorService,
            ITelemetryService telemetryService,
            IDateTimeService dateTimeService)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _telemetryService = telemetryService;
            _redactorService = redactorService;
            _dateTimeService = dateTimeService;
        }
        public async Task ExecuteAsync(string connectionString, string sql, object param = null, CommandType? commandType = null)
        {
            var methodIdentifier = $"{nameof(SQLService)}.{nameof(ExecuteAsync)}";
            var stopwatchService = _serviceProvider.GetRequiredService<IStopwatchService>();

            var telemetry = new TelemetryData
            {
                Name = sql,
                DateTime = _dateTimeService.GetDateTimeUTC(),
                TelemetryType = TelemetryType.SQL
            };

            try
            {
                stopwatchService.Start();
                using SqlConnection connection = new SqlConnection(connectionString);
                await connection.OpenAsync().ConfigureAwait(false);
                await connection.ExecuteAsync(
                    sql,
                    param,
                    commandType: commandType).ConfigureAwait(false);

                stopwatchService.Stop();
                telemetry.TelemetryState = TelemetryState.Successful;
                telemetry.ElapsedMilliseconds = (int)stopwatchService.ElapsedMilliseconds;

                _logger.LogInformationRedacted
                (
                    $"{methodIdentifier}",
                    new Dictionary<string, object>
                    {
                        { nameof(sql), sql },
                        { nameof(param), param },
                        { nameof(commandType), Enum.GetName(typeof(CommandType), commandType) },
                        { nameof(stopwatchService.ElapsedMilliseconds), telemetry.ElapsedMilliseconds }
                    }
                );
            }
            catch(Exception exception)
            {
                stopwatchService.Stop();
                telemetry.TelemetryState = TelemetryState.Failed;
                telemetry.ElapsedMilliseconds = (int)stopwatchService.ElapsedMilliseconds;

                _logger.LogErrorRedacted
                (
                    $"Unhandled exception {methodIdentifier}",
                    exception,
                    new Dictionary<string, object>
                    {
                        { nameof(sql), sql },
                        { nameof(param), param },
                        { nameof(commandType), Enum.GetName(typeof(CommandType), commandType) },
                        { nameof(stopwatchService.ElapsedMilliseconds), telemetry.ElapsedMilliseconds }
                    }
                );
                throw;
            }
            finally
            {
                _telemetryService.Insert(telemetry);
            }
        }

        public async Task<T> QueryFirstOrDefaultAsync<T>(string connectionString, string sql,  object param = null, CommandType? commandType = null)
        {
            var methodIdentifier = $"{nameof(SQLService)}.{nameof(QueryFirstOrDefaultAsync)}";
            var stopwatchService = _serviceProvider.GetRequiredService<IStopwatchService>();

            var telemetry = new TelemetryData
            {
                Name = sql,
                DateTime = _dateTimeService.GetDateTimeUTC(),
                TelemetryType = TelemetryType.SQL
            };

            try
            {
                stopwatchService.Start();
                using SqlConnection connection = new SqlConnection(connectionString);
                await connection.OpenAsync().ConfigureAwait(false);

                var result = await connection.QueryFirstOrDefaultAsync<T>(
                    sql,
                    param,
                    commandType: commandType).ConfigureAwait(false);


                stopwatchService.Stop();
                telemetry.TelemetryState = TelemetryState.Successful;
                telemetry.ElapsedMilliseconds = (int)stopwatchService.ElapsedMilliseconds;

                _logger.LogInformationRedacted
                (
                    $"{methodIdentifier}",
                    new Dictionary<string, object>
                    {
                            { nameof(sql), sql },
                            { nameof(param), param },
                            { nameof(commandType), Enum.GetName(typeof(CommandType), commandType) },
                            { nameof(stopwatchService.ElapsedMilliseconds), telemetry.ElapsedMilliseconds }
                    }
                );

                return result;
            }
            catch (Exception exception)
            {
                stopwatchService.Stop();
                telemetry.TelemetryState = TelemetryState.Failed;
                telemetry.ElapsedMilliseconds = (int)stopwatchService.ElapsedMilliseconds;
                _logger.LogErrorRedacted
                (
                    $"Unhandled exception {methodIdentifier}",
                    exception,
                    new Dictionary<string, object>
                    {
                        { nameof(sql), sql },
                        { nameof(param), param },
                        { nameof(commandType), Enum.GetName(typeof(CommandType), commandType) },
                        { nameof(stopwatchService.ElapsedMilliseconds), telemetry.ElapsedMilliseconds }
                    }
                );
                throw;
            }
            finally
            {
                _telemetryService.Insert(telemetry);
            }
        }

        public async Task<T> QueryFirstAsync<T>(string connectionString, string sql,  object param = null, CommandType? commandType = null)
        {
            var methodIdentifier = $"{nameof(SQLService)}.{nameof(QueryFirstAsync)}";
            var stopwatchService = _serviceProvider.GetRequiredService<IStopwatchService>();

            var telemetry = new TelemetryData
            {
                Name = sql,
                DateTime = _dateTimeService.GetDateTimeUTC(),
                TelemetryType = TelemetryType.SQL
            };

            try
            {
                using SqlConnection connection = new SqlConnection(connectionString);
                await connection.OpenAsync().ConfigureAwait(false);

                var result = await connection.QueryFirstAsync<T>(
                    sql,
                    param,
                    commandType: commandType).ConfigureAwait(false);

                stopwatchService.Stop();
                telemetry.TelemetryState = TelemetryState.Successful;
                telemetry.ElapsedMilliseconds = (int)stopwatchService.ElapsedMilliseconds;

                _logger.LogInformationRedacted
                (
                    $"{methodIdentifier}",
                    new Dictionary<string, object>
                    {
                        { nameof(sql), sql },
                        { nameof(param), param },
                        { nameof(commandType), Enum.GetName(typeof(CommandType), commandType) },
                        { nameof(stopwatchService.ElapsedMilliseconds), telemetry.ElapsedMilliseconds }
                    }
                );

                return result;
            }
            catch (Exception exception)
            {
                stopwatchService.Stop();
                telemetry.TelemetryState = TelemetryState.Failed;
                telemetry.ElapsedMilliseconds = (int)stopwatchService.ElapsedMilliseconds;
                _logger.LogErrorRedacted
                (
                    $"Unhandled exception {methodIdentifier}",
                    exception,
                    new Dictionary<string, object>
                    {
                        { nameof(sql), sql },
                        { nameof(param), param },
                        { nameof(commandType), Enum.GetName(typeof(CommandType), commandType) },
                        { nameof(stopwatchService.ElapsedMilliseconds), telemetry.ElapsedMilliseconds }
                    }
                );
                throw;
            }
            finally
            {
                _telemetryService.Insert(telemetry);
            }
        }

        public async Task<IEnumerable<T>> QueryAsync<T>(string connectionString, string sql, object param = null, CommandType? commandType = null)
        {
            var methodIdentifier = $"{nameof(SQLService)}.{nameof(QueryFirstOrDefaultAsync)}";
            var stopwatchService = _serviceProvider.GetRequiredService<IStopwatchService>();

            var telemetry = new TelemetryData
            {
                Name = sql,
                DateTime = _dateTimeService.GetDateTimeUTC(),
                TelemetryType = TelemetryType.SQL
            };

            try
            {
                using SqlConnection connection = new SqlConnection(connectionString);
                await connection.OpenAsync().ConfigureAwait(false);

                var result = await connection.QueryAsync<T>(
                    sql,
                    param,
                    commandType: commandType).ConfigureAwait(false);

                stopwatchService.Stop();
                telemetry.TelemetryState = TelemetryState.Successful;
                telemetry.ElapsedMilliseconds = (int)stopwatchService.ElapsedMilliseconds;

                _logger.LogInformationRedacted
                (
                    $"{methodIdentifier}",
                    new Dictionary<string, object>
                    {
                            { nameof(sql), sql },
                            { nameof(param), param },
                            { nameof(commandType), Enum.GetName(typeof(CommandType), commandType) },
                            { nameof(stopwatchService.ElapsedMilliseconds), telemetry.ElapsedMilliseconds }
                    }
                );

                return result;
            }
            catch (Exception exception)
            {
                stopwatchService.Stop();
                telemetry.TelemetryState = TelemetryState.Failed;
                telemetry.ElapsedMilliseconds = (int)stopwatchService.ElapsedMilliseconds;
                _logger.LogErrorRedacted
                (
                    $"Unhandled exception {methodIdentifier}",
                    exception,
                    new Dictionary<string, object>
                    {
                        { nameof(sql), sql },
                        { nameof(param), param },
                        { nameof(commandType), Enum.GetName(typeof(CommandType), commandType) },
                        { nameof(stopwatchService.ElapsedMilliseconds), telemetry.ElapsedMilliseconds }
                    }
                );
                throw;
            }
            finally
            {
                _telemetryService.Insert(telemetry);
            }
        }

        public async Task BulkCopyAsync<T>(string connectionString, DataTable table, string tableName, int? batchSize, TimeSpan? timeout, CancellationToken? token)
        {
            var methodIdentifier = $"{nameof(SQLService)}.{nameof(QueryFirstOrDefaultAsync)}";
            var stopwatchService = _serviceProvider.GetRequiredService<IStopwatchService>();

            var telemetry = new TelemetryData
            {
                Name = $"{methodIdentifier} - {tableName}",
                DateTime = _dateTimeService.GetDateTimeUTC(),
                TelemetryType = TelemetryType.SQL
            };

            if (table == null)
            {
                throw new ArgumentNullException(nameof(table));
            }
            if (table.Rows.Count == 0)
            {
                return;
            }

            try
            {
                using SqlConnection connection = new SqlConnection(connectionString);
                await connection.OpenAsync(token ?? CancellationToken.None).ConfigureAwait(false);

                using SqlBulkCopy bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, null)
                {
                    DestinationTableName = tableName,
                    BulkCopyTimeout = (int)(timeout ?? DefaultBulkCopyTimeout).TotalSeconds,
                    BatchSize = batchSize ?? DefaultBatchSize
                };
                for (int columnIndex = 0; columnIndex < table.Columns.Count; columnIndex++)
                {
                    DataColumn dataColumn = table.Columns[columnIndex];
                    bulkCopy.ColumnMappings.Add(dataColumn.ColumnName, dataColumn.ColumnName);
                }
                await bulkCopy.WriteToServerAsync(table, token ?? CancellationToken.None).ConfigureAwait(false);

                stopwatchService.Stop();
                telemetry.TelemetryState = TelemetryState.Successful;
                telemetry.ElapsedMilliseconds = (int)stopwatchService.ElapsedMilliseconds;

                _logger.LogInformationRedacted
                (
                    $"{methodIdentifier}",
                    new Dictionary<string, object>
                    {
                        { nameof(tableName), tableName },
                        { nameof(stopwatchService.ElapsedMilliseconds), telemetry.ElapsedMilliseconds },
                        { nameof(table.Rows), table.Rows.Count }
                    }
                );
            }
            catch (Exception exception)
            {
                stopwatchService.Stop();
                telemetry.TelemetryState = TelemetryState.Failed;
                telemetry.ElapsedMilliseconds = (int)stopwatchService.ElapsedMilliseconds;
                _logger.LogErrorRedacted
                (
                    $"Unhandled exception {methodIdentifier}",
                    exception,
                    new Dictionary<string, object>
                    {
                        { nameof(tableName), tableName },
                        { nameof(stopwatchService.ElapsedMilliseconds), telemetry.ElapsedMilliseconds },
                        { nameof(table.Rows), table.Rows }
                    }
                );
                throw;
            }
            finally
            {
                _telemetryService.Insert(telemetry);
            }
        }

    }

}
