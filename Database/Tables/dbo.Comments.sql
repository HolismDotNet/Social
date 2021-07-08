CREATE TABLE [dbo].[Comments]
(
[Id] [bigint] NOT NULL IDENTITY(1, 1),
[UserGuid] [uniqueidentifier] NOT NULL,
[Date] [datetime] NOT NULL,
[PersianDate] AS ([dbo].[ToPersianDateTime]([Date])),
[EntityTypeGuid] [uniqueidentifier] NOT NULL,
[EntityGuid] [uniqueidentifier] NOT NULL,
[Body] [nvarchar] (max) NOT NULL,
[IsApproved] [bit] NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [dbo].[Comments] ADD CONSTRAINT [PK_Comments] PRIMARY KEY CLUSTERED  ([Id]) ON [PRIMARY]
GO
