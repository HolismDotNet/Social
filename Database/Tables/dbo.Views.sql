CREATE TABLE [dbo].[Views]
(
[Id] [bigint] NOT NULL IDENTITY(1, 1),
[UserGuid] [uniqueidentifier] NOT NULL,
[EntityTypeGuid] [uniqueidentifier] NOT NULL,
[EntityGuid] [uniqueidentifier] NOT NULL
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Views] ADD CONSTRAINT [PK_Views] PRIMARY KEY CLUSTERED  ([Id]) ON [PRIMARY]
GO
