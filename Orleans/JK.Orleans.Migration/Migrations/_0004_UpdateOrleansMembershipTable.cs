using FluentMigrator;

namespace JK.Orleans.Migration.Migrations;

[FluentMigrator.Migration(2026022201, "Update Orleans membership table and refresh queries")]
public sealed class _0004_UpdateOrleansMembershipTable : FluentMigrator.Migration
{
    public override void Up()
    {
        // Execute.EmbeddedScript("_0004_UpdateOrleansMembershipTable_Up.sql");
        Execute.EmbeddedScript("SQLServer-Reminders.sql");
    }

    public override void Down()
    {
        // No down migration for refreshing queries
    }
}
