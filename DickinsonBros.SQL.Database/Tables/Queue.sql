CREATE TABLE [TestRunner].[Queue]
(
	[QueueId] INT NOT NULL PRIMARY KEY IDENTITY(1,1),
	[Payload] nvarchar(max) NOT NULL
)
