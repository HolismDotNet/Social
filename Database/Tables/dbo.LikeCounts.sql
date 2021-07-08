CREATE TABLE [dbo].[LikeCounts]
(
[Id] [bigint] NOT NULL IDENTITY(1, 1),
[EntityTypeGuid] [uniqueidentifier] NOT NULL,
[EntityGuid] [uniqueidentifier] NOT NULL,
[Count] [bigint] NOT NULL
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[LikeCounts] ADD CONSTRAINT [PK_LikeCounts] PRIMARY KEY CLUSTERED  ([Id]) ON [PRIMARY]
GO
ALTER TABLE [dbo].[LikeCounts] ADD CONSTRAINT [IX_LikeCounts_Unique_EntityGuid_And_EntityTypeGuid] UNIQUE NONCLUSTERED  ([EntityTypeGuid], [EntityGuid]) ON [PRIMARY]
GO
