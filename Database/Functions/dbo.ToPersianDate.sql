SET QUOTED_IDENTIFIER OFF
GO
SET ANSI_NULLS OFF
GO
CREATE FUNCTION [dbo].[ToPersianDate] (@value [datetime])
RETURNS [nvarchar] (100)
WITH EXECUTE AS CALLER
EXTERNAL NAME [HolismClrIntegration].[Holism.ClrIntegration.PersianDate].[ToPersianDate]
GO
