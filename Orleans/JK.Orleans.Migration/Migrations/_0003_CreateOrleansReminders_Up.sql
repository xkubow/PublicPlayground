IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[OrleansRemindersTable]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[OrleansRemindersTable](
        [ServiceId] NVARCHAR(150) NOT NULL,
        [GrainId] NVARCHAR(150) NOT NULL,
        [ReminderName] NVARCHAR(150) NOT NULL,
        [StartTime] DATETIME2 NOT NULL,
        [Period] INT NOT NULL,
        [GrainHash] INT NOT NULL,
        CONSTRAINT [PK_OrleansRemindersTable] PRIMARY KEY CLUSTERED ([ServiceId], [GrainId], [ReminderName])
    );

    CREATE INDEX [IX_OrleansReminders_GrainHash]
        ON [dbo].[OrleansRemindersTable] ([GrainHash]);
END;

-- Required for ADO.NET Reminders
INSERT INTO [dbo].[OrleansQuery] ([QueryKey], [QueryText])
SELECT 'UpsertReminderRowKey', '
    DECLARE @Succeeded AS BIT;
    SET @Succeeded = 0;
    BEGIN TRANSACTION;
    UPDATE OrleansRemindersTable WITH(UPDLOCK, HOLDLOCK)
    SET
        StartTime = @StartTime,
        Period = @Period,
        GrainHash = @GrainHash
    WHERE
        ServiceId = @ServiceId AND @ServiceId IS NOT NULL
        AND GrainId = @GrainId AND @GrainId IS NOT NULL
        AND ReminderName = @ReminderName AND @ReminderName IS NOT NULL;

    IF @@ROWCOUNT > 0
    BEGIN
        SET @Succeeded = 1;
    END
    ELSE
    BEGIN
        INSERT INTO OrleansRemindersTable
        (
            ServiceId,
            GrainId,
            ReminderName,
            StartTime,
            Period,
            GrainHash
        )
        SELECT @ServiceId, @GrainId, @ReminderName, @StartTime, @Period, @GrainHash;

        IF @@ROWCOUNT > 0
        BEGIN
            SET @Succeeded = 1;
        END
    END

    IF @Succeeded = 1
    BEGIN
        COMMIT TRANSACTION;
    END
    ELSE
    BEGIN
        ROLLBACK TRANSACTION;
    END

    SELECT @Succeeded;
' WHERE NOT EXISTS (SELECT 1 FROM [dbo].[OrleansQuery] WHERE [QueryKey] = 'UpsertReminderRowKey');

INSERT INTO [dbo].[OrleansQuery] ([QueryKey], [QueryText])
SELECT 'ReadReminderRowsKey', '
    SELECT
        GrainId,
        ReminderName,
        StartTime,
        Period,
        GrainHash
    FROM OrleansRemindersTable
    WHERE
        ServiceId = @ServiceId AND @ServiceId IS NOT NULL;
' WHERE NOT EXISTS (SELECT 1 FROM [dbo].[OrleansQuery] WHERE [QueryKey] = 'ReadReminderRowsKey');

INSERT INTO [dbo].[OrleansQuery] ([QueryKey], [QueryText])
SELECT 'ReadReminderRowKey', '
    SELECT
        GrainId,
        ReminderName,
        StartTime,
        Period,
        GrainHash
    FROM OrleansRemindersTable
    WHERE
        ServiceId = @ServiceId AND @ServiceId IS NOT NULL
        AND GrainId = @GrainId AND @GrainId IS NOT NULL
        AND ReminderName = @ReminderName AND @ReminderName IS NOT NULL;
' WHERE NOT EXISTS (SELECT 1 FROM [dbo].[OrleansQuery] WHERE [QueryKey] = 'ReadReminderRowKey');

INSERT INTO [dbo].[OrleansQuery] ([QueryKey], [QueryText])
SELECT 'ReadRangeRows1Key', '
    SELECT
        GrainId,
        ReminderName,
        StartTime,
        Period,
        GrainHash
    FROM OrleansRemindersTable
    WHERE
        ServiceId = @ServiceId AND @ServiceId IS NOT NULL
        AND GrainHash > @BeginHash AND GrainHash <= @EndHash;
' WHERE NOT EXISTS (SELECT 1 FROM [dbo].[OrleansQuery] WHERE [QueryKey] = 'ReadRangeRows1Key');

INSERT INTO [dbo].[OrleansQuery] ([QueryKey], [QueryText])
SELECT 'ReadRangeRows2Key', '
    SELECT
        GrainId,
        ReminderName,
        StartTime,
        Period,
        GrainHash
    FROM OrleansRemindersTable
    WHERE
        ServiceId = @ServiceId AND @ServiceId IS NOT NULL
        AND (GrainHash > @BeginHash OR GrainHash <= @EndHash);
' WHERE NOT EXISTS (SELECT 1 FROM [dbo].[OrleansQuery] WHERE [QueryKey] = 'ReadRangeRows2Key');

INSERT INTO [dbo].[OrleansQuery] ([QueryKey], [QueryText])
SELECT 'DeleteReminderRowKey', '
    DELETE FROM OrleansRemindersTable
    WHERE
        ServiceId = @ServiceId AND @ServiceId IS NOT NULL
        AND GrainId = @GrainId AND @GrainId IS NOT NULL
        AND ReminderName = @ReminderName AND @ReminderName IS NOT NULL;
    SELECT @@ROWCOUNT;
' WHERE NOT EXISTS (SELECT 1 FROM [dbo].[OrleansQuery] WHERE [QueryKey] = 'DeleteReminderRowKey');

INSERT INTO [dbo].[OrleansQuery] ([QueryKey], [QueryText])
SELECT 'DeleteReminderRowsKey', '
    DELETE FROM OrleansRemindersTable
    WHERE
        ServiceId = @ServiceId AND @ServiceId IS NOT NULL;
' WHERE NOT EXISTS (SELECT 1 FROM [dbo].[OrleansQuery] WHERE [QueryKey] = 'DeleteReminderRowsKey');
