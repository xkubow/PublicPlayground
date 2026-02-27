IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[OrleansStorage]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[OrleansStorage](
        [GrainIdHash] INT NOT NULL,
        [GrainIdNVer] INT NOT NULL,
        [GrainTypeHash] INT NOT NULL,
        [GrainTypeString] NVARCHAR(512) NOT NULL,
        [ServiceId] NVARCHAR(150) NOT NULL,
        [PayloadBinary] VARBINARY(MAX) NULL,
        [PayloadXml] NVARCHAR(MAX) NULL,
        [PayloadJson] NVARCHAR(MAX) NULL,
        [ModifiedOn] DATETIME2 NOT NULL,
        [ETag] NVARCHAR(150) NULL,
        CONSTRAINT [PK_OrleansStorage] PRIMARY KEY CLUSTERED ([GrainIdHash], [GrainIdNVer], [GrainTypeHash], [GrainTypeString], [ServiceId])
    );

    CREATE INDEX [IX_OrleansStorage] ON [dbo].[OrleansStorage] ([GrainIdHash], [GrainTypeHash]);
END;

DELETE FROM [dbo].[OrleansQuery] WHERE [QueryKey] = 'WriteToStorageKey';
INSERT INTO [dbo].[OrleansQuery] ([QueryKey], [QueryText])
VALUES ('WriteToStorageKey', '
    BEGIN TRANSACTION;
    SET NOCOUNT ON;
    DECLARE @Variables AS TABLE(CurrentETag NVARCHAR(150));
    UPDATE OrleansStorage WITH(UPDLOCK, HOLDLOCK)
    SET
        PayloadBinary = @PayloadBinary,
        PayloadXml = @PayloadXml,
        PayloadJson = @PayloadJson,
        ModifiedOn = SYSUTCDATETIME(),
        ETag = @ETag
    OUTPUT inserted.ETag INTO @Variables(CurrentETag)
    WHERE
        GrainIdHash = @GrainIdHash AND @GrainIdHash IS NOT NULL
        AND GrainIdNVer = @GrainIdNVer AND @GrainIdNVer IS NOT NULL
        AND GrainTypeHash = @GrainTypeHash AND @GrainTypeHash IS NOT NULL
        AND GrainTypeString = @GrainTypeString AND @GrainTypeString IS NOT NULL
        AND ServiceId = @ServiceId AND @ServiceId IS NOT NULL
        AND (ETag = @OldETag OR @OldETag IS NULL OR ETag IS NULL);

    IF @@ROWCOUNT > 0
    BEGIN
        SELECT CurrentETag FROM @Variables;
        COMMIT TRANSACTION;
    END
    ELSE
    BEGIN
        INSERT INTO OrleansStorage
        (
            GrainIdHash,
            GrainIdNVer,
            GrainTypeHash,
            GrainTypeString,
            ServiceId,
            PayloadBinary,
            PayloadXml,
            PayloadJson,
            ModifiedOn,
            ETag
        )
        SELECT
            @GrainIdHash, @GrainIdNVer, @GrainTypeHash, @GrainTypeString, @ServiceId,
            @PayloadBinary, @PayloadXml, @PayloadJson, SYSUTCDATETIME(), @ETag
        WHERE NOT EXISTS
        (
            SELECT 1 FROM OrleansStorage
            WHERE
                GrainIdHash = @GrainIdHash AND @GrainIdHash IS NOT NULL
                AND GrainIdNVer = @GrainIdNVer AND @GrainIdNVer IS NOT NULL
                AND GrainTypeHash = @GrainTypeHash AND @GrainTypeHash IS NOT NULL
                AND GrainTypeString = @GrainTypeString AND @GrainTypeString IS NOT NULL
                AND ServiceId = @ServiceId AND @ServiceId IS NOT NULL
        );

        IF @@ROWCOUNT > 0
        BEGIN
            SELECT @ETag;
            COMMIT TRANSACTION;
        END
        ELSE
        BEGIN
            ROLLBACK TRANSACTION;
        END
    END
');

DELETE FROM [dbo].[OrleansQuery] WHERE [QueryKey] = 'ReadFromStorageKey';
INSERT INTO [dbo].[OrleansQuery] ([QueryKey], [QueryText])
VALUES ('ReadFromStorageKey', '
    SELECT
        PayloadBinary,
        PayloadXml,
        PayloadJson,
        ETag
    FROM
        OrleansStorage
    WHERE
        GrainIdHash = @GrainIdHash AND @GrainIdHash IS NOT NULL
        AND GrainIdNVer = @GrainIdNVer AND @GrainIdNVer IS NOT NULL
        AND GrainTypeHash = @GrainTypeHash AND @GrainTypeHash IS NOT NULL
        AND GrainTypeString = @GrainTypeString AND @GrainTypeString IS NOT NULL
        AND ServiceId = @ServiceId AND @ServiceId IS NOT NULL;
');

DELETE FROM [dbo].[OrleansQuery] WHERE [QueryKey] = 'ClearStorageKey';
INSERT INTO [dbo].[OrleansQuery] ([QueryKey], [QueryText])
VALUES ('ClearStorageKey', '
    BEGIN TRANSACTION;
    SET NOCOUNT ON;
    UPDATE OrleansStorage WITH(UPDLOCK, HOLDLOCK)
    SET
        PayloadBinary = NULL,
        PayloadXml = NULL,
        PayloadJson = NULL,
        ModifiedOn = SYSUTCDATETIME(),
        ETag = @ETag
    WHERE
        GrainIdHash = @GrainIdHash AND @GrainIdHash IS NOT NULL
        AND GrainIdNVer = @GrainIdNVer AND @GrainIdNVer IS NOT NULL
        AND GrainTypeHash = @GrainTypeHash AND @GrainTypeHash IS NOT NULL
        AND GrainTypeString = @GrainTypeString AND @GrainTypeString IS NOT NULL
        AND ServiceId = @ServiceId AND @ServiceId IS NOT NULL
        AND (ETag = @OldETag OR @OldETag IS NULL OR ETag IS NULL);

    IF @@ROWCOUNT > 0
    BEGIN
        SELECT @ETag;
        COMMIT TRANSACTION;
    END
    ELSE
    BEGIN
        ROLLBACK TRANSACTION;
    END
');
