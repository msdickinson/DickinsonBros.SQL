CREATE TABLE [TestRunner].[Mixed]
(
	[MixedId] INT NOT NULL PRIMARY KEY IDENTITY(1,1),
	[Bool] BIT NOT NULL, 
    [Int] INT NOT NULL, 
    [Float] FLOAT NOT NULL, 
    [Double] BIGINT NOT NULL, 
    [SampleEnum] INT NOT NULL, 
    [Char] NVARCHAR NOT NULL, 
    [Byte] tinyint NOT NULL, 
    [ByteArray] VARBINARY(MAX) NOT NULL, 
    [Guid] UNIQUEIDENTIFIER NOT NULL, 
    [DateTime] DATETIME2 NOT NULL, 
    [TimeSpan] TIME NOT NULL, 
    [String] NVARCHAR(MAX) NOT NULL,
    [NullValueType] BIT NULL, 
    [NullString] NCHAR(10) NULL
)
