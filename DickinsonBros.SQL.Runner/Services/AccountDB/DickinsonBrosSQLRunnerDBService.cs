using Dapper;
using DickinsonBros.SQL.Abstractions;
using DickinsonBros.SQL.Runner.Models;
using DickinsonBros.SQL.Runner.Services.AccountDB.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DickinsonBros.SQL.Runner.Services.AccountDB
{
    public class DickinsonBrosSQLRunnerDBService : IDickinsonBrosSQLRunnerDBService
    {
        internal readonly string _connectionString;
        internal readonly ISQLService _sqlService;

        internal const string QUEUE_TABLE_NAME = "[DickinsonBros.SQL.Runner.Database].[TestRunner].[Queue]";

        internal const string SELECT_TOP_1_BY_QUEUEID_DESC = 
 @"SELECT TOP (1) 
    [QueueId],
    [Payload]
FROM [DickinsonBros.SQL.Runner.Database].[TestRunner].[Queue]
order by QueueId DESC;";

        internal const string SELECT_TOP_50_BY_QUEUEID_DESC =
@"SELECT TOP (50) 
    [QueueId],
    [Payload]
FROM [DickinsonBros.SQL.Runner.Database].[TestRunner].[Queue]
order by QueueId DESC;";


        internal const string UPDATE_QUEUE_DATA_BY_QUEUEID_DESC =
@"UPDATE [DickinsonBros.SQL.Runner.Database].[TestRunner].[Queue]
        SET [Payload] = @Payload
 WHERE QueueId = @QueueId";

        internal const string DELETE_ALL_QUEUE_ITEMS =
@"DELETE FROM [DickinsonBros.SQL.Runner.Database].[TestRunner].[Queue];";

        internal const string INSERT_QUEUE_ITEM =
@"INSERT INTO [DickinsonBros.SQL.Runner.Database].[TestRunner].[Queue]
        ([Payload])
    VALUES
        (@Payload);";

        internal const string INSERT_QUEUE_ITEMS =
@"[DickinsonBros.SQL.Runner.Database].[TestRunner].InsertQueueItems ";

        internal const string SELECT_TOP_1_BY_QUEUEID_DESC_WITH_STORED_PROC = "[TestRunner].[SelectTopOneQueue]";

        public DickinsonBrosSQLRunnerDBService
        (
            IOptions<DickinsonBrosDBOptions> dickinsonBrosDBOptions,
            ISQLService sqlService
        )
        {
            _connectionString = dickinsonBrosDBOptions.Value.ConnectionString;
            _sqlService = sqlService;
        }

        public async Task<QueueDTO> QueryQueueFirstOrDefaultAsync()
        {
            return await _sqlService
                        .QueryFirstOrDefaultAsync<QueueDTO>
                         (
                             _connectionString,
                             SELECT_TOP_1_BY_QUEUEID_DESC,
                             commandType: CommandType.Text
                         ).ConfigureAwait(false);
        }

        public async Task<QueueDTO> QueryQueueFirstAsync()
        {
            return await _sqlService
                        .QueryFirstAsync<QueueDTO>
                         (
                             _connectionString,
                             SELECT_TOP_1_BY_QUEUEID_DESC,
                             commandType: CommandType.Text
                         ).ConfigureAwait(false);
        }

        public async Task<List<QueueDTO>> SelectLast50QueueItemsProc()
        {
            return (await _sqlService
                        .QueryAsync<QueueDTO>
                         (
                             _connectionString,
                             SELECT_TOP_50_BY_QUEUEID_DESC,
                             commandType: CommandType.Text
                         ).ConfigureAwait(false)).ToList();
        }

        public async Task UpdateQueueItemAsync(QueueDTO queueItem)
        {
            await _sqlService
                  .ExecuteAsync
                  (
                      _connectionString,
                      UPDATE_QUEUE_DATA_BY_QUEUEID_DESC,
                      queueItem,
                      commandType: CommandType.Text
                  ).ConfigureAwait(false);
        }

        public async Task DeleteAllQueueItemsAsync()
        {
            await _sqlService
                  .ExecuteAsync
                  (
                      _connectionString,
                      DELETE_ALL_QUEUE_ITEMS,
                      commandType: CommandType.Text
                  ).ConfigureAwait(false);
        }

        public async Task InsertQueueItemAsync(QueueDTO queueItem)
        {
            await _sqlService
                  .ExecuteAsync
                  (
                      _connectionString,
                      INSERT_QUEUE_ITEM,
                      queueItem,
                      commandType: CommandType.Text
                  ).ConfigureAwait(false);
        }

        public async Task InsertQueueItemsAsync(List<QueueDTO> queueItems)
        {
            var dataTable = new DataTable();

            dataTable.Columns.Add(new DataColumn("Payload", typeof(string)));
            queueItems.ForEach(e => dataTable.Rows.Add(e.Payload));

            await _sqlService
                  .ExecuteAsync
                  (
                      _connectionString,
                      INSERT_QUEUE_ITEMS,
                      new
                      {
                          @QueueItems = dataTable.AsTableValuedParameter()
                      },
                      commandType: CommandType.StoredProcedure
                  ).ConfigureAwait(false);
        }

        public async Task BulkInsertQueueItemsAsync(List<QueueDTO> queueItems)
        {
            var dataTable = new DataTable();

            dataTable.Columns.Add(new DataColumn("Payload", typeof(string)));

            queueItems.ForEach(e => dataTable.Rows.Add(e.Payload));

            await _sqlService
                  .BulkCopyAsync<QueueDTO>
                  (
                      _connectionString,
                      dataTable,
                      QUEUE_TABLE_NAME,
                      null,
                      null,
                      null
                  ).ConfigureAwait(false);
        }

        public async Task SelectLatestQueueItemOrDefaultWithStoredProc()
        {
            await _sqlService
                  .ExecuteAsync
                  (
                      _connectionString,
                      SELECT_TOP_1_BY_QUEUEID_DESC_WITH_STORED_PROC,
                      commandType: CommandType.StoredProcedure
                  ).ConfigureAwait(false);
        }

    }

    public class InsertQueueDTOS
    {
        public DataTable queueItems;
    }
}
