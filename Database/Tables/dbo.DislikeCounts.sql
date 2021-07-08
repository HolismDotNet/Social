CREATE TABLE [dbo].[DislikeCounts]
(
[Id] [bigint] NOT NULL IDENTITY(1, 1),
[EntityTypeGuid] [uniqueidentifier] NOT NULL,
[EntityGuid] [uniqueidentifier] NOT NULL,
[Count] [bigint] NOT NULL
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[DislikeCounts] ADD CONSTRAINT [PK_DislikeCounts] PRIMARY KEY CLUSTERED  ([Id]) ON [PRIMARY]
GO
ALTER TABLE [dbo].[DislikeCounts] ADD CONSTRAINT [IX_DislikeCounts_Unique_EntityGuid_And_EntityTypeGuid] UNIQUE NONCLUSTERED  ([EntityTypeGuid], [EntityGuid]) ON [PRIMARY]
GO
