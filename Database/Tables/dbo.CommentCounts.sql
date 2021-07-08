CREATE TABLE [dbo].[CommentCounts]
(
[Id] [bigint] NOT NULL IDENTITY(1, 1),
[EntityTypeGuid] [uniqueidentifier] NOT NULL,
[EntityGuid] [uniqueidentifier] NOT NULL,
[Count] [bigint] NOT NULL
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[CommentCounts] ADD CONSTRAINT [PK_CommentCounts] PRIMARY KEY CLUSTERED  ([Id]) ON [PRIMARY]
GO
ALTER TABLE [dbo].[CommentCounts] ADD CONSTRAINT [IX_CommentCounts_Unique_EntityGuid_And_EntityTypeGuid] UNIQUE NONCLUSTERED  ([EntityTypeGuid], [EntityGuid]) ON [PRIMARY]
GO
