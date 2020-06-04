# DickinsonBros.SQL

<a href="https://www.nuget.org/packages/DickinsonBros.SQL/">
    <img src="https://img.shields.io/nuget/v/DickinsonBros.SQL">
</a>

SQL Service

Features
* Methods: Execute, QueryFirst, QueryFirstOrDefault, Query, and BulkCopy
* Logs for all successful and exceptional runs
* Telemetry for all calls

<a href="https://dev.azure.com/marksamdickinson/DickinsonBros/_build?definitionScope=%5CDickinsonBros.SQL">Builds</a>

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
      
Note: Logs can be redacted via configuration (see https://github.com/msdickinson/DickinsonBros.Redactor)

Telemetry generated when using DickinsonBros.Telemetry and connecting it to a configured database for ITelemetry 
See https://github.com/msdickinson/DickinsonBros.Telemetry on how to configure DickinsonBros.Telemetry and setup the database.

![Alt text](https://raw.githubusercontent.com/msdickinson/DickinsonBros.SQL/develop/TelemetrySQLSample.PNG)


<h2>Setup</h2>

<h3>Add nuget references</h3>

    https://www.nuget.org/packages/DickinsonBros.DateTime
    https://www.nuget.org/packages/DickinsonBros.DateTime.Abstractions
    
    https://www.nuget.org/packages/DickinsonBros.Stopwatch
    https://www.nuget.org/packages/DickinsonBros.Stopwatch.Abstractions
    
    https://www.nuget.org/packages/DickinsonBros.Telemetry
    https://www.nuget.org/packages/DickinsonBros.Telemetry.Abstractions

    https://www.nuget.org/packages/DickinsonBros.Telemetry
    https://www.nuget.org/packages/DickinsonBros.Telemetry.Abstractions
    
    https://www.nuget.org/packages/DickinsonBros.Logger
    https://www.nuget.org/packages/DickinsonBros.Logger.Abstractions
    
    https://www.nuget.org/packages/DickinsonBros.Redactor
    https://www.nuget.org/packages/DickinsonBros.Redactor.Abstractions

<h3>Create instance with dependency injection</h3>

<h4>Add appsettings.json File With Contents</h4>

Note: Runner Shows this with added steps to enypct Connection String

 ```json  
{
  "TelemetryServiceOptions": {
    "ConnectionString": ""
  },
  "RedactorServiceOptions": {
    "PropertiesToRedact": [
      ""
    ],
    "RegexValuesToRedact": []
  }
}
 ```    
<h4>Code</h4>

```c#

//ApplicationLifetime
using var applicationLifetime = new ApplicationLifetime();

//ServiceCollection
var serviceCollection = new ServiceCollection();

//Configure Options
var builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", false)

var configuration = builder.Build();
serviceCollection.AddOptions();

services.AddSingleton<IApplicationLifetime>(applicationLifetime);

//Add DateTime Service
services.AddDateTimeService();

//Add Stopwatch Service
services.AddStopwatchService();

//Add Logging Service
services.AddLoggingService();

//Add Redactor
services.AddRedactorService();
services.Configure<RedactorServiceOptions>(_configuration.GetSection(nameof(RedactorServiceOptions)));

//Add Telemetry
services.AddTelemetryService();
services.Configure<TelemetryServiceOptions>(_configuration.GetSection(nameof(TelemetryServiceOptions)));

//Add SQLService
services.AddSQLService();

//Build Service Provider 
using (var provider = services.BuildServiceProvider())
{
  var sqlService = provider.GetRequiredService<ISQLService>();
}
```
