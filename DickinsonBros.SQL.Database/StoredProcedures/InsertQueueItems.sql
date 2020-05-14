CREATE PROCEDURE [TestRunner].InsertQueueItems
    @QueueItems [TestRunner].[QueueInsertType] readonly
AS
BEGIN
INSERT INTO [TestRunner].[Queue] (PayLoad)
   SELECT PayLoad
   FROM @QueueItems;
END