USE [techsite_cfg-2.X.X]
GO

INSERT INTO [dbo].[GTP_LIST]
	SELECT * FROM [10.100.104.18].[techsite_cfg-2.X.X].[dbo].[GTP_LIST]

USE [techsite_cfg-2.X.X]
GO

INSERT INTO [dbo].[ID_TG_ASKUE_BiTEC]
	SELECT * FROM [10.100.104.18].[techsite_cfg-2.X.X].[dbo].[ID_TG_ASKUE_BiTEC]

USE [techsite_cfg-2.X.X]
GO

INSERT INTO [dbo].[ID_TG_ASKUE_COMMON]
	SELECT * FROM [10.100.104.18].[techsite_cfg-2.X.X].[dbo].[ID_TG_ASKUE_COMMON]
	
USE [techsite_cfg-2.X.X]
GO

INSERT INTO [dbo].[ID_TG_SOTIASSO]
	SELECT * FROM [10.100.104.18].[techsite_cfg-2.X.X].[dbo].[ID_TG_SOTIASSO]
	
USE [techsite_cfg-2.X.X]
GO

INSERT INTO [dbo].[ID_TSN_ASKUE]
	SELECT * FROM [10.100.104.18].[techsite_cfg-2.X.X].[dbo].[ID_TSN_ASKUE]
	
USE [techsite_cfg-2.X.X]
GO

INSERT INTO [dbo].[ID_TSN_SOTIASSO]
	SELECT * FROM [10.100.104.18].[techsite_cfg-2.X.X].[dbo].[ID_TSN_SOTIASSO]
	
USE [techsite_cfg-2.X.X]
GO

INSERT INTO [dbo].[passwords]
	SELECT * FROM [10.100.104.18].[techsite_cfg-2.X.X].[dbo].[passwords]
	
USE [techsite_cfg-2.X.X]
GO

INSERT INTO [dbo].[PC_LIST]
	SELECT * FROM [10.100.104.18].[techsite_cfg-2.X.X].[dbo].[PC_LIST]
	
USE [techsite_cfg-2.X.X]
GO

INSERT INTO [dbo].[roles]
	SELECT * FROM [10.100.104.18].[techsite_cfg-2.X.X].[dbo].[roles]


USE [techsite_cfg-2.X.X]
GO

INSERT INTO [techsite_cfg-2.X.X].[dbo].[setup]
           ([VALUE]
           ,[KEY]
           ,[LAST_UPADTE]
           ,[ID_UNIT])
     SELECT [VALUE]
           ,[KEY]
           ,[LAST_UPADTE]
           ,[ID_UNIT]
     FROM [10.100.104.18].[techsite_cfg-2.X.X].[dbo].[setup]
GO


USE [techsite_cfg-2.X.X]
GO

INSERT INTO [dbo].[SOURCE]
	SELECT * FROM [10.100.104.18].[techsite_cfg-2.X.X].[dbo].[SOURCE]

USE [techsite_cfg-2.X.X]
GO

INSERT INTO [dbo].[TEC_LIST]
	SELECT * FROM [10.100.104.18].[techsite_cfg-2.X.X].[dbo].[TEC_LIST]
	
USE [techsite_cfg-2.X.X]
GO

INSERT INTO [dbo].[TG_LIST]
	SELECT * FROM [10.100.104.18].[techsite_cfg-2.X.X].[dbo].[TG_LIST]
	
USE [techsite_cfg-2.X.X]
GO

INSERT INTO [dbo].[users]
	SELECT * FROM [10.100.104.18].[techsite_cfg-2.X.X].[dbo].[users]