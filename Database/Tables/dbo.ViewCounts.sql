CREATE TABLE [dbo].[ViewCounts]
(
[Id] [bigint] NOT NULL IDENTITY(1, 1),
[EntityTypeGuid] [uniqueidentifier] NOT NULL,
[EntityGuid] [uniqueidentifier] NOT NULL,
[Count] [bigint] NOT NULL
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[ViewCounts] ADD CONSTRAINT [PK_ViewCounts] PRIMARY KEY CLUSTERED  ([Id]) ON [PRIMARY]
GO
ALTER TABLE [dbo].[ViewCounts] ADD CONSTRAINT [IX_ViewCounts_Unique_EntityGuid_And_EntityTypeGuid] UNIQUE NONCLUSTERED  ([EntityTypeGuid], [EntityGuid]) ON [PRIMARY]
GO
