USE [techsite_cfg-2.X.X]
GO

/****** Object:  UserDefinedFunction [dbo].[ft_Date-Versions_ID_TG_ASKUE_Bitec]    Script Date: 11/28/2014 13:44:30 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO




CREATE FUNCTION [dbo].[ft_Date-Versions_ID_TG_ASKUE_Bitec]
(
@number_version int = 0 
)
RETURNS TABLE
AS RETURN
(
SELECT [LAST_UPDATE] FROM [dbo].[v_Versions_ID_TG_ASKUE_BiTEC] WHERE [NUMBER_VERSION] = @number_version
)



GO

USE [techsite_cfg-2.X.X]
GO

/****** Object:  UserDefinedFunction [dbo].[ft_ID_TG_ASKUE]    Script Date: 11/28/2014 13:44:48 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



CREATE FUNCTION [dbo].[ft_ID_TG_ASKUE]
(
@number_version int = 0 
)
RETURNS TABLE
AS RETURN
(
SELECT [ID_TEC]
      ,[SENSORS_NAME]
      ,[LAST_UPDATE]
      ,[ID_TG]
      ,[ID_3]
      ,[ID_30]
FROM [dbo].[ID_TG_ASKUE_COMMON]
UNION ALL
SELECT [ID_TEC]
      ,[SENSORS_NAME]
      ,[LAST_UPDATE]
      ,[ID_TG]
      ,[ID_3]
      ,[ID_30]
FROM [dbo].[ID_TG_ASKUE_BiTEC]
WHERE [LAST_UPDATE] = (SELECT [LAST_UPDATE] FROM [dbo].[ft_Date-Versions_ID_TG_ASKUE_Bitec](@number_version))
)




GO



USE [techsite_cfg-2.X.X]
GO

/****** Object:  UserDefinedFunction [dbo].[ft_ALL_PARAM_TG]    Script Date: 11/28/2014 13:45:09 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO





CREATE FUNCTION [dbo].[ft_ALL_PARAM_TG]
(
@number_version_askue_bitec int = 0 
)
RETURNS TABLE
AS RETURN
(
SELECT 
       [tg_list].[ID] AS [ID_TG]
	  ,[REALS].[ID_REALS_RV] AS [ID_IN_SOTIASSO]
	  ,[ID_TG_ASKUE].[ID_3] AS [ID_IN_ASKUE_3]
	  ,[ID_TG_ASKUE].[ID_30] AS [ID_IN_ASKUE_30]
      ,[tg_list].[NAME_SHR] AS [NAME_SHR_TG]
      ,[tg_list].[PREFIX] AS [PREFIX_TG]
      ,[tg_list].[INDX_COL_RDG_EXCEL] AS [INDX_COL_RDG_EXCEL_TG]
	  ,[tg_list].[ID_PC] AS [ID_PC]
	  ,[PC_LIST].[NAME_SHR] AS [NAME_SHR_PC]
	  ,[tec_list].[ID] AS [ID_TEC]
      ,[tec_list].[NAME_SHR] AS [NAME_SHR_TEC]
      ,[tec_list].[PREFIX_PBR]
	  ,[tec_list].[TEMPLATE_NAME_SGN_DATA_TM]
	  ,[tec_list].[TEMPLATE_NAME_SGN_DATA_FACT]
	  ,[tec_list].[TIMEZONE_OFFSET_MOSCOW]
	  ,[tec_list].[PATH_RDG_EXCEL]
	  ,[gtp_list].[ID] AS [ID_GTP]
      ,[gtp_list].[NAME_SHR] AS [NAME_SHR_GTP]
      ,[gtp_list].[NAME_FUTURE] AS [NAME_FUTURE_GTP]
      ,[gtp_list].[PREFIX_ADMIN] AS [PREFIX_ADMIN_GTP]
      ,[gtp_list].[PREFIX_PBR] AS [PREFIX_PBR_GTP]
FROM [techsite_cfg-2.X.X].[dbo].[TEC_LIST] AS [tec_list] JOIN
[techsite_cfg-2.X.X].[dbo].[GTP_LIST] AS [gtp_list] ON [tec_list].[ID] = [gtp_list].[ID_TEC] JOIN
[techsite_cfg-2.X.X].[dbo].[TG_LIST] AS [tg_list] ON ([tec_list].[ID] = [tg_list].[ID_TEC] AND [gtp_list].[ID] = [tg_list].[ID_GTP]) JOIN
[techsite_cfg-2.X.X].[dbo].[ID_TG_SOTIASSO] AS [REALS] ON [tg_list].[ID] = [REALS].[ID_TG] JOIN
[techsite_cfg-2.X.X].[dbo].[ft_ID_TG_ASKUE](@number_version_askue_bitec) AS [ID_TG_ASKUE] ON ([ID_TG_ASKUE].[ID_TG] = [tg_list].[ID] AND [ID_TG_ASKUE].[ID_TEC] = [tg_list].[ID_TEC]) JOIN
[techsite_cfg-2.X.X].[dbo].[PC_LIST] AS [PC_LIST] ON ([PC_LIST].[ID] = [tg_list].[ID_PC] AND [PC_LIST].[ID_TEC] = [tg_list].[ID_TEC])
)






GO

