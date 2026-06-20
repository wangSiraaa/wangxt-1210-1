USE PowerSwitchDb;
GO

DECLARE @ups1 UNIQUEIDENTIFIER = NEWID();
DECLARE @ups2 UNIQUEIDENTIFIER = NEWID();
DECLARE @bus1 UNIQUEIDENTIFIER = NEWID();
DECLARE @bus2 UNIQUEIDENTIFIER = NEWID();
DECLARE @bus3 UNIQUEIDENTIFIER = NEWID();
DECLARE @bus4 UNIQUEIDENTIFIER = NEWID();

IF NOT EXISTS (SELECT 1 FROM DeviceTopologies WHERE DeviceCode = 'UPS-A01')
INSERT INTO DeviceTopologies (Id, DeviceType, DeviceCode, DeviceName, Location, ParentId, HasDualPower, RouteASource, RouteBSource, ConnectedBusinessSystems)
VALUES (@ups1, 1, 'UPS-A01', '1号UPS', 'A区配电室1层', NULL, 1, '市电A路', '市电B路', NULL);

IF NOT EXISTS (SELECT 1 FROM DeviceTopologies WHERE DeviceCode = 'UPS-B01')
INSERT INTO DeviceTopologies (Id, DeviceType, DeviceCode, DeviceName, Location, ParentId, HasDualPower, RouteASource, RouteBSource, ConnectedBusinessSystems)
VALUES (@ups2, 1, 'UPS-B01', '2号UPS', 'B区配电室1层', NULL, 1, '市电A路', '市电B路', NULL);

IF NOT EXISTS (SELECT 1 FROM DeviceTopologies WHERE DeviceCode = 'BUS-A1')
INSERT INTO DeviceTopologies (Id, DeviceType, DeviceCode, DeviceName, Location, ParentId, HasDualPower, RouteASource, RouteBSource, ConnectedBusinessSystems)
VALUES (@bus1, 2, 'BUS-A1', 'A区1段母线', 'A区配电室1层', @ups1, 1, 'UPS-A01-A路', 'UPS-B01-B路', NULL);

IF NOT EXISTS (SELECT 1 FROM DeviceTopologies WHERE DeviceCode = 'BUS-A2')
INSERT INTO DeviceTopologies (Id, DeviceType, DeviceCode, DeviceName, Location, ParentId, HasDualPower, RouteASource, RouteBSource, ConnectedBusinessSystems)
VALUES (@bus2, 2, 'BUS-A2', 'A区2段母线', 'A区配电室1层', @ups1, 1, 'UPS-A01-A路', 'UPS-B01-B路', NULL);

IF NOT EXISTS (SELECT 1 FROM DeviceTopologies WHERE DeviceCode = 'BUS-B1')
INSERT INTO DeviceTopologies (Id, DeviceType, DeviceCode, DeviceName, Location, ParentId, HasDualPower, RouteASource, RouteBSource, ConnectedBusinessSystems)
VALUES (@bus3, 2, 'BUS-B1', 'B区1段母线', 'B区配电室1层', @ups2, 1, 'UPS-A01-A路', 'UPS-B01-B路', NULL);

IF NOT EXISTS (SELECT 1 FROM DeviceTopologies WHERE DeviceCode = 'BUS-B2')
INSERT INTO DeviceTopologies (Id, DeviceType, DeviceCode, DeviceName, Location, ParentId, HasDualPower, RouteASource, RouteBSource, ConnectedBusinessSystems)
VALUES (@bus4, 2, 'BUS-B2', 'B区2段母线', 'B区配电室1层', @ups2, 1, 'UPS-A01-A路', 'UPS-B01-B路', NULL);

IF NOT EXISTS (SELECT 1 FROM DeviceTopologies WHERE DeviceCode = 'CAB-A1-01')
INSERT INTO DeviceTopologies (Id, DeviceType, DeviceCode, DeviceName, Location, ParentId, HasDualPower, RouteASource, RouteBSource, ConnectedBusinessSystems)
VALUES (NEWID(), 3, 'CAB-A1-01', 'A1-01机柜', 'A区机房1排1列', @bus1, 1, 'BUS-A1-A路', 'BUS-A2-B路', '交易系统,支付网关');

IF NOT EXISTS (SELECT 1 FROM DeviceTopologies WHERE DeviceCode = 'CAB-A1-02')
INSERT INTO DeviceTopologies (Id, DeviceType, DeviceCode, DeviceName, Location, ParentId, HasDualPower, RouteASource, RouteBSource, ConnectedBusinessSystems)
VALUES (NEWID(), 3, 'CAB-A1-02', 'A1-02机柜', 'A区机房1排2列', @bus1, 1, 'BUS-A1-A路', 'BUS-A2-B路', '核心交易数据库');

IF NOT EXISTS (SELECT 1 FROM DeviceTopologies WHERE DeviceCode = 'CAB-A1-03')
INSERT INTO DeviceTopologies (Id, DeviceType, DeviceCode, DeviceName, Location, ParentId, HasDualPower, RouteASource, RouteBSource, ConnectedBusinessSystems)
VALUES (NEWID(), 3, 'CAB-A1-03', 'A1-03机柜', 'A区机房1排3列', @bus2, 1, 'BUS-A2-A路', 'BUS-A1-B路', '风控系统,报表系统');

IF NOT EXISTS (SELECT 1 FROM DeviceTopologies WHERE DeviceCode = 'CAB-B1-01')
INSERT INTO DeviceTopologies (Id, DeviceType, DeviceCode, DeviceName, Location, ParentId, HasDualPower, RouteASource, RouteBSource, ConnectedBusinessSystems)
VALUES (NEWID(), 3, 'CAB-B1-01', 'B1-01机柜', 'B区机房1排1列', @bus3, 1, 'BUS-B1-A路', 'BUS-B2-B路', '客户系统,用户中心');

IF NOT EXISTS (SELECT 1 FROM DeviceTopologies WHERE DeviceCode = 'CAB-B1-02')
INSERT INTO DeviceTopologies (Id, DeviceType, DeviceCode, DeviceName, Location, ParentId, HasDualPower, RouteASource, RouteBSource, ConnectedBusinessSystems)
VALUES (NEWID(), 3, 'CAB-B1-02', 'B1-02机柜', 'B区机房1排2列', @bus3, 1, 'BUS-B1-A路', 'BUS-B2-B路', '消息中间件集群');

IF NOT EXISTS (SELECT 1 FROM DeviceTopologies WHERE DeviceCode = 'CAB-B1-03')
INSERT INTO DeviceTopologies (Id, DeviceType, DeviceCode, DeviceName, Location, ParentId, HasDualPower, RouteASource, RouteBSource, ConnectedBusinessSystems)
VALUES (NEWID(), 3, 'CAB-B1-03', 'B1-03机柜', 'B区机房1排3列', @bus4, 1, 'BUS-B2-A路', 'BUS-B1-B路', '日志分析,监控平台');

PRINT 'Seed data inserted.';
