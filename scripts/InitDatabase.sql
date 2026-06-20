IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'PowerSwitchDb')
BEGIN
    CREATE DATABASE PowerSwitchDb;
END
GO

USE PowerSwitchDb;
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'DeviceTopologies')
BEGIN
    CREATE TABLE DeviceTopologies (
        Id UNIQUEIDENTIFIER PRIMARY KEY,
        DeviceType INT NOT NULL,
        DeviceCode NVARCHAR(100) NOT NULL UNIQUE,
        DeviceName NVARCHAR(200) NOT NULL,
        Location NVARCHAR(500) NULL,
        ParentId UNIQUEIDENTIFIER NULL FOREIGN KEY REFERENCES DeviceTopologies(Id),
        HasDualPower BIT NOT NULL DEFAULT 0,
        RouteASource NVARCHAR(100) NULL,
        RouteBSource NVARCHAR(100) NULL,
        ConnectedBusinessSystems NVARCHAR(200) NULL
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PowerSwitchRequests')
BEGIN
    CREATE TABLE PowerSwitchRequests (
        Id UNIQUEIDENTIFIER PRIMARY KEY,
        RequestNo NVARCHAR(50) NOT NULL UNIQUE,
        Title NVARCHAR(200) NOT NULL,
        Description NVARCHAR(1000) NULL,
        Status INT NOT NULL DEFAULT 0,
        RiskWindowStart DATETIME2 NOT NULL,
        RiskWindowEnd DATETIME2 NOT NULL,
        DutyManagerName NVARCHAR(100) NULL,
        DutyManagerFilledAt DATETIME2 NULL,
        EngineerName NVARCHAR(100) NULL,
        EngineerFilledAt DATETIME2 NULL,
        BusinessOwnerName NVARCHAR(100) NULL,
        BusinessConfirmedAt DATETIME2 NULL,
        IsLowPeakConfirmed BIT NOT NULL DEFAULT 0,
        LowPeakRemark NVARCHAR(500) NULL,
        DualPowerCheckPassed BIT NOT NULL DEFAULT 0,
        DualPowerCheckedAt DATETIME2 NULL,
        DualPowerCheckRemark NVARCHAR(500) NULL,
        ExecutionStartedAt DATETIME2 NULL,
        ExecutionCompletedAt DATETIME2 NULL,
        RollbackReason NVARCHAR(2000) NULL,
        RolledBackAt DATETIME2 NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AffectedDevices')
BEGIN
    CREATE TABLE AffectedDevices (
        Id UNIQUEIDENTIFIER PRIMARY KEY,
        RequestId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES PowerSwitchRequests(Id) ON DELETE CASCADE,
        DeviceType INT NOT NULL,
        DeviceCode NVARCHAR(100) NOT NULL,
        DeviceName NVARCHAR(200) NOT NULL,
        Location NVARCHAR(500) NULL,
        ImpactDescription NVARCHAR(1000) NULL
    );
    CREATE INDEX IX_AffectedDevices_RequestId ON AffectedDevices(RequestId);
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SwitchSteps')
BEGIN
    CREATE TABLE SwitchSteps (
        Id UNIQUEIDENTIFIER PRIMARY KEY,
        RequestId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES PowerSwitchRequests(Id) ON DELETE CASCADE,
        Sequence INT NOT NULL,
        StepType NVARCHAR(100) NOT NULL,
        Description NVARCHAR(500) NOT NULL,
        OperationDetail NVARCHAR(1000) NULL,
        EstimatedDurationSeconds INT NOT NULL DEFAULT 0,
        Status INT NOT NULL DEFAULT 0,
        StartedAt DATETIME2 NULL,
        CompletedAt DATETIME2 NULL,
        OperatorName NVARCHAR(100) NULL,
        Remark NVARCHAR(1000) NULL,
        IsRollbackStep BIT NOT NULL DEFAULT 0
    );
    CREATE INDEX IX_SwitchSteps_RequestId ON SwitchSteps(RequestId);
    CREATE UNIQUE INDEX IX_SwitchSteps_RequestId_Sequence ON SwitchSteps(RequestId, Sequence);
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AlarmRecords')
BEGIN
    CREATE TABLE AlarmRecords (
        Id UNIQUEIDENTIFIER PRIMARY KEY,
        RequestId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES PowerSwitchRequests(Id) ON DELETE CASCADE,
        Severity INT NOT NULL,
        AlarmMessage NVARCHAR(500) NOT NULL,
        SourceDevice NVARCHAR(200) NULL,
        AlarmTime DATETIME2 NOT NULL,
        IsConfirmed BIT NOT NULL DEFAULT 0,
        ConfirmedBy NVARCHAR(100) NULL,
        ConfirmedAt DATETIME2 NULL,
        ConfirmRemark NVARCHAR(1000) NULL
    );
    CREATE INDEX IX_AlarmRecords_RequestId ON AlarmRecords(RequestId);
    CREATE INDEX IX_AlarmRecords_IsConfirmed ON AlarmRecords(IsConfirmed);
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'RollbackRecords')
BEGIN
    CREATE TABLE RollbackRecords (
        Id UNIQUEIDENTIFIER PRIMARY KEY,
        RequestId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES PowerSwitchRequests(Id) ON DELETE CASCADE,
        Sequence INT NOT NULL,
        Description NVARCHAR(500) NOT NULL,
        OperationDetail NVARCHAR(1000) NULL,
        Status INT NOT NULL DEFAULT 0,
        StartedAt DATETIME2 NULL,
        CompletedAt DATETIME2 NULL,
        OperatorName NVARCHAR(100) NULL
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'BusinessImpacts')
BEGIN
    CREATE TABLE BusinessImpacts (
        Id UNIQUEIDENTIFIER PRIMARY KEY,
        RequestId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES PowerSwitchRequests(Id) ON DELETE CASCADE,
        BusinessSystemName NVARCHAR(200) NOT NULL,
        SystemCode NVARCHAR(100) NULL,
        AffectedCabinetCount INT NOT NULL DEFAULT 0,
        ActualImpactStart DATETIME2 NULL,
        ActualImpactEnd DATETIME2 NULL,
        ImpactDescription NVARCHAR(1000) NULL,
        RecoveryDetail NVARCHAR(1000) NULL,
        VerifiedBy NVARCHAR(100) NULL,
        VerifiedAt DATETIME2 NULL
    );
    CREATE INDEX IX_BusinessImpacts_RequestId ON BusinessImpacts(RequestId);
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'DualPowerCheckRecords')
BEGIN
    CREATE TABLE DualPowerCheckRecords (
        Id UNIQUEIDENTIFIER PRIMARY KEY,
        RequestId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES PowerSwitchRequests(Id) ON DELETE CASCADE,
        DeviceCode NVARCHAR(100) NOT NULL,
        DeviceType INT NOT NULL,
        RouteAPowered BIT NOT NULL DEFAULT 0,
        RouteBPowered BIT NOT NULL DEFAULT 0,
        BothRoutesHealthy BIT NOT NULL DEFAULT 0,
        CheckRemark NVARCHAR(1000) NULL,
        CheckedBy NVARCHAR(100) NULL,
        CheckedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
    CREATE INDEX IX_DualPowerCheckRecords_RequestId ON DualPowerCheckRecords(RequestId);
END
GO

PRINT 'Database initialization complete.';
