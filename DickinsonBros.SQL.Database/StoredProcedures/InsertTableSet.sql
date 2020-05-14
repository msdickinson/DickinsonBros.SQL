CREATE PROCEDURE [TestRunner].SelectTopOneQueue
as

select top (1) *
from [TestRunner].[Queue]
order by QueueId DESC

RETURN 0
