using FluentMigrator;

namespace JK.Orleans.Migration.Migrations;

[FluentMigrator.Migration(2026021203, "Create Orleans reminders table")] 
public sealed class _0003_CreateOrleansReminders : FluentMigrator.Migration
{
    public override void Up()
    {
        // Execute.EmbeddedScript("_0003_CreateOrleansReminders_Up.sql");
        Execute.EmbeddedScript("SQLServer-Persistence.sql");
    }

    public override void Down()
    {
        Execute.EmbeddedScript("_0003_CreateOrleansReminders_Down.sql");
    }
}
