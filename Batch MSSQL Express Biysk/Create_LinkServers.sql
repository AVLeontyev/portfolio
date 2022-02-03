/****** Object:  LinkedServer [10.100.104.248]    Script Date: 11/28/2014 10:26:55 ******/
IF  EXISTS (SELECT srv.name FROM sys.servers srv WHERE srv.server_id != 0 AND srv.name = N'10.100.104.248')EXEC master.dbo.sp_dropserver @server=N'10.100.104.248', @droplogins='droplogins'
GO

/****** Object:  LinkedServer [10.100.104.248]    Script Date: 11/28/2014 10:26:55 ******/
EXEC master.dbo.sp_addlinkedserver @server = N'10.100.104.248', @srvproduct=N'SQL Server'
 /* For security reasons the linked server remote logins password is changed with ######## */
EXEC master.dbo.sp_addlinkedsrvlogin @rmtsrvname=N'10.100.104.248',@useself=N'False',@locallogin=NULL,@rmtuser=N'cllient',@rmtpassword=N'client'

GO

EXEC master.dbo.sp_serveroption @server=N'10.100.104.248', @optname=N'collation compatible', @optvalue=N'false'
GO

EXEC master.dbo.sp_serveroption @server=N'10.100.104.248', @optname=N'data access', @optvalue=N'true'
GO

EXEC master.dbo.sp_serveroption @server=N'10.100.104.248', @optname=N'dist', @optvalue=N'false'
GO

EXEC master.dbo.sp_serveroption @server=N'10.100.104.248', @optname=N'pub', @optvalue=N'false'
GO

EXEC master.dbo.sp_serveroption @server=N'10.100.104.248', @optname=N'rpc', @optvalue=N'false'
GO

EXEC master.dbo.sp_serveroption @server=N'10.100.104.248', @optname=N'rpc out', @optvalue=N'false'
GO

EXEC master.dbo.sp_serveroption @server=N'10.100.104.248', @optname=N'sub', @optvalue=N'false'
GO

EXEC master.dbo.sp_serveroption @server=N'10.100.104.248', @optname=N'connect timeout', @optvalue=N'0'
GO

EXEC master.dbo.sp_serveroption @server=N'10.100.104.248', @optname=N'collation name', @optvalue=null
GO

EXEC master.dbo.sp_serveroption @server=N'10.100.104.248', @optname=N'lazy schema validation', @optvalue=N'false'
GO

EXEC master.dbo.sp_serveroption @server=N'10.100.104.248', @optname=N'query timeout', @optvalue=N'0'
GO

EXEC master.dbo.sp_serveroption @server=N'10.100.104.248', @optname=N'use remote collation', @optvalue=N'true'
GO

EXEC master.dbo.sp_serveroption @server=N'10.100.104.248', @optname=N'remote proc transaction promotion', @optvalue=N'true'
GO


/****** Object:  LinkedServer [10.220.2.60]    Script Date: 11/28/2014 13:38:18 ******/
EXEC master.dbo.sp_addlinkedserver @server = N'10.220.2.60', @srvproduct=N'SQL Server'
 /* For security reasons the linked server remote logins password is changed with ######## */
EXEC master.dbo.sp_addlinkedsrvlogin @rmtsrvname=N'10.220.2.60',@useself=N'False',@locallogin=NULL,@rmtuser=N'client',@rmtpassword='client'

GO

EXEC master.dbo.sp_serveroption @server=N'10.220.2.60', @optname=N'collation compatible', @optvalue=N'false'
GO

EXEC master.dbo.sp_serveroption @server=N'10.220.2.60', @optname=N'data access', @optvalue=N'true'
GO

EXEC master.dbo.sp_serveroption @server=N'10.220.2.60', @optname=N'dist', @optvalue=N'false'
GO

EXEC master.dbo.sp_serveroption @server=N'10.220.2.60', @optname=N'pub', @optvalue=N'false'
GO

EXEC master.dbo.sp_serveroption @server=N'10.220.2.60', @optname=N'rpc', @optvalue=N'false'
GO

EXEC master.dbo.sp_serveroption @server=N'10.220.2.60', @optname=N'rpc out', @optvalue=N'false'
GO

EXEC master.dbo.sp_serveroption @server=N'10.220.2.60', @optname=N'sub', @optvalue=N'false'
GO

EXEC master.dbo.sp_serveroption @server=N'10.220.2.60', @optname=N'connect timeout', @optvalue=N'0'
GO

EXEC master.dbo.sp_serveroption @server=N'10.220.2.60', @optname=N'collation name', @optvalue=null
GO

EXEC master.dbo.sp_serveroption @server=N'10.220.2.60', @optname=N'lazy schema validation', @optvalue=N'false'
GO

EXEC master.dbo.sp_serveroption @server=N'10.220.2.60', @optname=N'query timeout', @optvalue=N'0'
GO

EXEC master.dbo.sp_serveroption @server=N'10.220.2.60', @optname=N'use remote collation', @optvalue=N'true'
GO

EXEC master.dbo.sp_serveroption @server=N'10.220.2.60', @optname=N'remote proc transaction promotion', @optvalue=N'true'
GO


/****** Object:  LinkedServer [192.168.2.20]    Script Date: 11/28/2014 13:38:51 ******/
EXEC master.dbo.sp_addlinkedserver @server = N'192.168.2.20', @srvproduct=N'SQL Server'
 /* For security reasons the linked server remote logins password is changed with ######## */
EXEC master.dbo.sp_addlinkedsrvlogin @rmtsrvname=N'192.168.2.20',@useself=N'False',@locallogin=NULL,@rmtuser=N'client',@rmtpassword='client'

GO

EXEC master.dbo.sp_serveroption @server=N'192.168.2.20', @optname=N'collation compatible', @optvalue=N'false'
GO

EXEC master.dbo.sp_serveroption @server=N'192.168.2.20', @optname=N'data access', @optvalue=N'true'
GO

EXEC master.dbo.sp_serveroption @server=N'192.168.2.20', @optname=N'dist', @optvalue=N'false'
GO

EXEC master.dbo.sp_serveroption @server=N'192.168.2.20', @optname=N'pub', @optvalue=N'false'
GO

EXEC master.dbo.sp_serveroption @server=N'192.168.2.20', @optname=N'rpc', @optvalue=N'false'
GO

EXEC master.dbo.sp_serveroption @server=N'192.168.2.20', @optname=N'rpc out', @optvalue=N'false'
GO

EXEC master.dbo.sp_serveroption @server=N'192.168.2.20', @optname=N'sub', @optvalue=N'false'
GO

EXEC master.dbo.sp_serveroption @server=N'192.168.2.20', @optname=N'connect timeout', @optvalue=N'0'
GO

EXEC master.dbo.sp_serveroption @server=N'192.168.2.20', @optname=N'collation name', @optvalue=null
GO

EXEC master.dbo.sp_serveroption @server=N'192.168.2.20', @optname=N'lazy schema validation', @optvalue=N'false'
GO

EXEC master.dbo.sp_serveroption @server=N'192.168.2.20', @optname=N'query timeout', @optvalue=N'0'
GO

EXEC master.dbo.sp_serveroption @server=N'192.168.2.20', @optname=N'use remote collation', @optvalue=N'true'
GO

EXEC master.dbo.sp_serveroption @server=N'192.168.2.20', @optname=N'remote proc transaction promotion', @optvalue=N'true'
GO

