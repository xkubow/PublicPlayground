IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[OrleansQuery]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[OrleansQuery](
        [QueryKey] NVARCHAR(64) NOT NULL PRIMARY KEY,
        [QueryText] NVARCHAR(MAX) NOT NULL
    );
END;

DELETE FROM [dbo].[OrleansQuery] WHERE [QueryKey] = 'GatewaysQueryKey';
INSERT INTO [dbo].[OrleansQuery] ([QueryKey], [QueryText])
VALUES ('GatewaysQueryKey', '
    SELECT
        Address,
        ProxyPort AS Port,
        Generation
    FROM
        OrleansMembershipTable
    WHERE
        DeploymentId = @DeploymentId AND @DeploymentId IS NOT NULL
        AND (Status = 3 OR Status = @Status)
        AND ProxyPort > 0;
');

DELETE FROM [dbo].[OrleansQuery] WHERE [QueryKey] = 'MembershipReadRowKey';
INSERT INTO [dbo].[OrleansQuery] ([QueryKey], [QueryText])
VALUES ('MembershipReadRowKey', '
    SELECT
        Address,
        Port,
        Generation,
        SiloName,
        HostName,
        Status,
        ProxyPort,
        StartTime,
        IAmAliveTime,
        SuspectTimes
    FROM
        OrleansMembershipTable
    WHERE
        DeploymentId = @DeploymentId AND @DeploymentId IS NOT NULL
        AND Address = @Address AND @Address IS NOT NULL
        AND Port = @Port AND @Port IS NOT NULL
        AND Generation = @Generation AND @Generation IS NOT NULL;

    SELECT
        Version
    FROM
        OrleansMembershipVersionTable
    WHERE
        DeploymentId = @DeploymentId AND @DeploymentId IS NOT NULL;
');

DELETE FROM [dbo].[OrleansQuery] WHERE [QueryKey] = 'MembershipReadAllKey';
INSERT INTO [dbo].[OrleansQuery] ([QueryKey], [QueryText])
VALUES ('MembershipReadAllKey', '
    SELECT
        Address,
        Port,
        Generation,
        SiloName,
        HostName,
        Status,
        ProxyPort,
        StartTime,
        IAmAliveTime,
        SuspectTimes
    FROM
        OrleansMembershipTable
    WHERE
        DeploymentId = @DeploymentId AND @DeploymentId IS NOT NULL;

    SELECT
        Version
    FROM
        OrleansMembershipVersionTable
    WHERE
        DeploymentId = @DeploymentId AND @DeploymentId IS NOT NULL;
');

DELETE FROM [dbo].[OrleansQuery] WHERE [QueryKey] = 'InsertMembershipVersionKey';
INSERT INTO [dbo].[OrleansQuery] ([QueryKey], [QueryText])
VALUES ('InsertMembershipVersionKey', '
    INSERT INTO OrleansMembershipVersionTable
    (
        DeploymentId,
        Version,
        VersionTime
    )
    SELECT
        @DeploymentId,
        0,
        SYSUTCDATETIME()
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM OrleansMembershipVersionTable
        WHERE DeploymentId = @DeploymentId AND @DeploymentId IS NOT NULL
    );

    SELECT @@ROWCOUNT;
');

DELETE FROM [dbo].[OrleansQuery] WHERE [QueryKey] = 'UpdateIAmAlivetimeKey';
INSERT INTO [dbo].[OrleansQuery] ([QueryKey], [QueryText])
VALUES ('UpdateIAmAlivetimeKey', '
    UPDATE OrleansMembershipTable
    SET
        IAmAliveTime = @IAmAliveTime
    WHERE
        DeploymentId = @DeploymentId AND @DeploymentId IS NOT NULL
        AND Address = @Address AND @Address IS NOT NULL
        AND Port = @Port AND @Port IS NOT NULL
        AND Generation = @Generation AND @Generation IS NOT NULL;
');

DELETE FROM [dbo].[OrleansQuery] WHERE [QueryKey] = 'InsertMembershipKey';
INSERT INTO [dbo].[OrleansQuery] ([QueryKey], [QueryText])
VALUES ('InsertMembershipKey', '
    BEGIN TRANSACTION;
    UPDATE OrleansMembershipVersionTable
    SET
        Version = Version + 1,
        VersionTime = SYSUTCDATETIME()
    WHERE
        DeploymentId = @DeploymentId AND @DeploymentId IS NOT NULL
        AND Version = @Version AND @Version IS NOT NULL;

    IF @@ROWCOUNT > 0
    BEGIN
        INSERT INTO OrleansMembershipTable
        (
            DeploymentId,
            Address,
            Port,
            Generation,
            SiloName,
            HostName,
            Status,
            ProxyPort,
            StartTime,
            IAmAliveTime
        )
        SELECT
            @DeploymentId,
            @Address,
            @Port,
            @Generation,
            @SiloName,
            @HostName,
            @Status,
            @ProxyPort,
            @StartTime,
            @IAmAliveTime
        WHERE NOT EXISTS
        (
            SELECT 1
            FROM OrleansMembershipTable
            WHERE
                DeploymentId = @DeploymentId AND @DeploymentId IS NOT NULL
                AND Address = @Address AND @Address IS NOT NULL
                AND Port = @Port AND @Port IS NOT NULL
                AND Generation = @Generation AND @Generation IS NOT NULL
        );
        IF @@ROWCOUNT > 0
        BEGIN
            COMMIT TRANSACTION;
            SELECT Version FROM OrleansMembershipVersionTable WHERE DeploymentId = @DeploymentId AND @DeploymentId IS NOT NULL;
        END
        ELSE
        BEGIN
            ROLLBACK TRANSACTION;
        END
    END
    ELSE
    BEGIN
        ROLLBACK TRANSACTION;
    END
');

DELETE FROM [dbo].[OrleansQuery] WHERE [QueryKey] = 'UpdateMembershipKey';
INSERT INTO [dbo].[OrleansQuery] ([QueryKey], [QueryText])
VALUES ('UpdateMembershipKey', '
    BEGIN TRANSACTION;
    UPDATE OrleansMembershipVersionTable
    SET
        Version = Version + 1,
        VersionTime = SYSUTCDATETIME()
    WHERE
        DeploymentId = @DeploymentId AND @DeploymentId IS NOT NULL
        AND Version = @Version AND @Version IS NOT NULL;

    IF @@ROWCOUNT > 0
    BEGIN
        UPDATE OrleansMembershipTable
        SET
            Status = @Status,
            SuspectTimes = @SuspectTimes,
            IAmAliveTime = @IAmAliveTime
        WHERE
            DeploymentId = @DeploymentId AND @DeploymentId IS NOT NULL
            AND Address = @Address AND @Address IS NOT NULL
            AND Port = @Port AND @Port IS NOT NULL
            AND Generation = @Generation AND @Generation IS NOT NULL;

        IF @@ROWCOUNT > 0
        BEGIN
            COMMIT TRANSACTION;
            SELECT Version FROM OrleansMembershipVersionTable WHERE DeploymentId = @DeploymentId AND @DeploymentId IS NOT NULL;
        END
        ELSE
        BEGIN
            ROLLBACK TRANSACTION;
        END
    END
    ELSE
    BEGIN
        ROLLBACK TRANSACTION;
    END
');

DELETE FROM [dbo].[OrleansQuery] WHERE [QueryKey] = 'DeleteMembershipTableEntriesKey';
INSERT INTO [dbo].[OrleansQuery] ([QueryKey], [QueryText])
VALUES ('DeleteMembershipTableEntriesKey', '
    DELETE FROM OrleansMembershipTable
    WHERE DeploymentId = @DeploymentId AND @DeploymentId IS NOT NULL;

    DELETE FROM OrleansMembershipVersionTable
    WHERE DeploymentId = @DeploymentId AND @DeploymentId IS NOT NULL;
');

DELETE FROM [dbo].[OrleansQuery] WHERE [QueryKey] = 'CleanupDefunctSiloEntriesKey';
INSERT INTO [dbo].[OrleansQuery] ([QueryKey], [QueryText])
VALUES ('CleanupDefunctSiloEntriesKey', '
    DELETE FROM OrleansMembershipTable
    WHERE DeploymentId = @DeploymentId AND @DeploymentId IS NOT NULL
        AND Status = @Status AND @Status IS NOT NULL
        AND IAmAliveTime < @IAmAliveTime AND @IAmAliveTime IS NOT NULL;
');

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[OrleansMembershipTable]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[OrleansMembershipTable](
        [DeploymentId] NVARCHAR(150) NOT NULL,
        [Address] NVARCHAR(45) NOT NULL,
        [Port] INT NOT NULL,
        [Generation] INT NOT NULL,
        [SiloName] NVARCHAR(150) NOT NULL,
        [HostName] NVARCHAR(150) NOT NULL,
        [Status] INT NOT NULL,
        [ProxyPort] INT NULL,
        [StartTime] DATETIME2 NOT NULL,
        [IAmAliveTime] DATETIME2 NOT NULL,
        [SuspectTimes] NVARCHAR(MAX) NULL,
        CONSTRAINT [PK_OrleansMembershipTable] PRIMARY KEY CLUSTERED ([DeploymentId], [Address], [Port], [Generation])
    );
END;

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[OrleansMembershipVersionTable]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[OrleansMembershipVersionTable](
        [DeploymentId] NVARCHAR(150) NOT NULL PRIMARY KEY,
        [Version] INT NOT NULL,
        [VersionTime] DATETIME2 NOT NULL
    );
END;
