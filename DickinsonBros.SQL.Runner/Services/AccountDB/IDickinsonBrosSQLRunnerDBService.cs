using DickinsonBros.SQL.Runner.Services.AccountDB.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DickinsonBros.SQL.Runner.Services.AccountDB
{
    public interface IDickinsonBrosSQLRunnerDBService
    {
        Task BulkInsertQueueItemsAsync(List<QueueDTO> queueItems);
        Task DeleteAllQueueItemsAsync();
        Task InsertQueueItemAsync(QueueDTO queueItem);
        Task InsertQueueItemsAsync(List<QueueDTO> queueItems);
        Task<List<QueueDTO>> SelectLast50QueueItemsProc();
        Task<QueueDTO> QueryQueueFirstOrDefaultAsync();
        Task SelectLatestQueueItemOrDefaultWithStoredProc();
        Task<QueueDTO> QueryQueueFirstAsync();
        Task UpdateQueueItemAsync(QueueDTO queueItem);
    }
}