USE [RCS.Caching]
GO
/****** Object:  Table [dbo].[CacheEntries]    Script Date: 2025/05/15 06:53:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CacheEntries]
(
	[Key] [nvarchar](450) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[Value] [nvarchar](max) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[ExpiresOn] [datetime] NULL,

 PRIMARY KEY NONCLUSTERED HASH 
(
	[Key]
)WITH ( BUCKET_COUNT = 131072)
)WITH ( MEMORY_OPTIMIZED = ON , DURABILITY = SCHEMA_AND_DATA )
GO
