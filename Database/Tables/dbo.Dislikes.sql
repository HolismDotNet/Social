CREATE TABLE [dbo].[Dislikes]
(
[Id] [bigint] NOT NULL IDENTITY(1, 1),
[UserGuid] [uniqueidentifier] NOT NULL,
[EntityTypeGuid] [uniqueidentifier] NOT NULL,
[EntityGuid] [uniqueidentifier] NOT NULL
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Dislikes] ADD CONSTRAINT [PK_Dislikes] PRIMARY KEY CLUSTERED  ([Id]) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Dislikes] ADD CONSTRAINT [IX_Dislikes_Unique_EntityGuid_And_EntityTypeGuid_And_UserGuid] UNIQUE NONCLUSTERED  ([UserGuid], [EntityTypeGuid], [EntityGuid]) ON [PRIMARY]
GO
