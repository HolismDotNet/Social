SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
create function [dbo].[GetFqn](@objectId bigint)
returns varchar(100)
as
begin
	declare @fqn varchar(100);
	select @fqn = quotename(object_schema_name(@objectId)) + '.' + quotename(object_name(@objectId));
	return @fqn;
end
GO
