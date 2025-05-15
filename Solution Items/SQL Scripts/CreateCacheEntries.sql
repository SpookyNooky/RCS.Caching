-- 1. Set context to the RCS.Caching database
USE [RCS.Caching];
GO

-- 2. Add memory-optimized filegroup (if not already present)
IF NOT EXISTS (
    SELECT 1
    FROM sys.filegroups
    WHERE type_desc = 'MEMORY_OPTIMIZED_DATA_FILEGROUP'
)
BEGIN
    ALTER DATABASE [RCS.Caching]
    ADD FILEGROUP MemOptimizedFG CONTAINS MEMORY_OPTIMIZED_DATA;

    ALTER DATABASE [RCS.Caching]
    ADD FILE (
        NAME = 'MemOptimizedData',
        FILENAME = 'C:\Program Files\Microsoft SQL Server\MSSQL16.MSSQLSERVER\MSSQL\DATA\MemOptimizedData'
    ) TO FILEGROUP MemOptimizedFG;

    PRINT 'Memory-optimized filegroup added.';
END
ELSE
BEGIN
    PRINT 'Memory-optimized filegroup already exists.';
END
GO

-- 3. Drop old table if exists
IF OBJECT_ID('dbo.CacheEntries', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.CacheEntries;
    PRINT 'Old CacheEntries table dropped.';
END
GO

-- 4. Create memory-optimized table
CREATE TABLE dbo.CacheEntries
(
    [Key] NVARCHAR(450) NOT NULL
        PRIMARY KEY NONCLUSTERED HASH WITH (BUCKET_COUNT = 100000),
    [Value] NVARCHAR(MAX) NOT NULL,
    [ExpiresOn] DATETIME NULL
)
WITH (MEMORY_OPTIMIZED = ON, DURABILITY = SCHEMA_AND_DATA);
GO

PRINT 'Memory-optimized CacheEntries table created.';
