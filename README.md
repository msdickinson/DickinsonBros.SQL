# DickinsonBros.SQL
<a href="https://dev.azure.com/marksamdickinson/dickinsonbros/_build/latest?definitionId=17&amp;branchName=master"> <img alt="Azure DevOps builds (branch)" src="https://img.shields.io/azure-devops/build/marksamdickinson/DickinsonBros/17/master"> </a> <a href="https://dev.azure.com/marksamdickinson/dickinsonbros/_build/latest?definitionId=17&amp;branchName=master"> <img alt="Azure DevOps coverage (branch)" src="https://img.shields.io/azure-devops/coverage/marksamdickinson/dickinsonbros/17/master"> </a><a href="https://dev.azure.com/marksamdickinson/DickinsonBros/_release?_a=releases&view=mine&definitionId=8"> <img alt="Azure DevOps releases" src="https://img.shields.io/azure-devops/release/marksamdickinson/b5a46403-83bb-4d18-987f-81b0483ef43e/8/9"> </a><a href="https://www.nuget.org/packages/DickinsonBros.SQL/"><img src="https://img.shields.io/nuget/v/DickinsonBros.SQL"></a>

SQL Service

Features
* Methods: Execute, QueryFirst, QueryFirstOrDefault, Query, and BulkCopy
* Logs for all successful and exceptional runs
* Telemetry for all calls

<h2>Example Usage</h2>

Note: Example below is based on Example Runner that contains a warper class on SQLService called DickinsonBrosSQLRunnerDBService. https://github.com/msdickinson/DickinsonBros.SQL/tree/develop/DickinsonBros.SQL.Runner


```C#
var queueItem = new QueueDTO
{
    Payload = @"{""X"": ""1""}"
};
var queueItems = new List<QueueDTO>
{
    queueItem,
    queueItem,
    queueItem,
    queueItem,
    queueItem
};

//ExecuteAsync (Delete, Insert Item, Insert Items)
await dickinsonBrosSQLRunnerDBService.DeleteAllQueueItemsAsync().ConfigureAwait(false);
await dickinsonBrosSQLRunnerDBService.InsertQueueItemAsync(queueItem).ConfigureAwait(false);
await dickinsonBrosSQLRunnerDBService.InsertQueueItemsAsync(queueItems).ConfigureAwait(false);

//BulkCopyAsync
await dickinsonBrosSQLRunnerDBService.BulkInsertQueueItemsAsync(queueItems).ConfigureAwait(false);

//QueryFirstAsync
var queueItemObserved = await dickinsonBrosSQLRunnerDBService.QueryQueueFirstAsync().ConfigureAwait(false);

//QueryFirstOrDefaultAsync
var queueItemOrDefaultObserved = await dickinsonBrosSQLRunnerDBService.QueryQueueFirstOrDefaultAsync().ConfigureAwait(false);

//QueryAsync
var queueItemsObserved = await dickinsonBrosSQLRunnerDBService.SelectLast50QueueItemsProc().ConfigureAwait(false);

//ExecuteAsync (Update)
queueItemObserved.Payload = @"{""X"": ""2""}";
await dickinsonBrosSQLRunnerDBService.UpdateQueueItemAsync(queueItemObserved).ConfigureAwait(false);

await telemetryService.FlushAsync().ConfigureAwait(false);
```

    info: DickinsonBros.SQL.SQLService[1]
          SQLService.ExecuteAsync
          sql: DELETE FROM [DickinsonBros.SQL.Runner.Database].[TestRunner].[Queue];
          param:
          commandType: Text
          ElapsedMilliseconds: 1021

    info: DickinsonBros.SQL.SQLService[1]
          SQLService.ExecuteAsync
          sql: INSERT INTO [DickinsonBros.SQL.Runner.Database].[TestRunner].[Queue]
                  ([Payload])
              VALUES
                  (@Payload);
          param: {
            "QueueId": 0,
            "Payload": "{\r\n  \"X\": \"1\"\r\n}"
          }
          commandType: Text
          ElapsedMilliseconds: 63

    info: DickinsonBros.SQL.SQLService[1]
          SQLService.ExecuteAsync
          sql: [DickinsonBros.SQL.Runner.Database].[TestRunner].InsertQueueItems
          param: {
            "QueueItems": {}
          }
          commandType: StoredProcedure
          ElapsedMilliseconds: 35

    info: DickinsonBros.SQL.SQLService[1]
          SQLService.QueryFirstOrDefaultAsync
          tableName: [DickinsonBros.SQL.Runner.Database].[TestRunner].[Queue]
          ElapsedMilliseconds: 0
          Rows: 5

    info: DickinsonBros.SQL.SQLService[1]
          SQLService.QueryFirstAsync
          sql: SELECT TOP (1)
              [QueueId],
              [Payload]
          FROM [DickinsonBros.SQL.Runner.Database].[TestRunner].[Queue]
          order by QueueId DESC;
          param:
          commandType: Text
          ElapsedMilliseconds: 0

    info: DickinsonBros.SQL.SQLService[1]
          SQLService.QueryFirstOrDefaultAsync
          sql: SELECT TOP (1)
              [QueueId],
              [Payload]
          FROM [DickinsonBros.SQL.Runner.Database].[TestRunner].[Queue]
          order by QueueId DESC;
          param:
          commandType: Text
          ElapsedMilliseconds: 1

    info: DickinsonBros.SQL.SQLService[1]
          SQLService.QueryFirstOrDefaultAsync
          sql: SELECT TOP (50)
              [QueueId],
              [Payload]
          FROM [DickinsonBros.SQL.Runner.Database].[TestRunner].[Queue]
          order by QueueId DESC;
          param:
          commandType: Text
          ElapsedMilliseconds: 0

    info: DickinsonBros.SQL.SQLService[1]
          SQLService.ExecuteAsync
          sql: UPDATE [DickinsonBros.SQL.Runner.Database].[TestRunner].[Queue]
                  SET [Payload] = @Payload
           WHERE QueueId = @QueueId
          param: {
            "QueueId": 1166,
            "Payload": "{\r\n  \"X\": \"2\"\r\n}"
          }
          commandType: Text
          ElapsedMilliseconds: 2
      
<b>Telemetry</b>

![Alt text](https://raw.githubusercontent.com/msdickinson/DickinsonBros.SQL/develop/TelemetrySQLSample.PNG)

[Sample Runner](https://github.com/msdickinson/DickinsonBros.SQL/tree/master/DickinsonBros.SQL.Runner)
