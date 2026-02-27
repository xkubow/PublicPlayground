using FluentMigrator;

namespace JK.Orleans.Migration.Migrations;

[FluentMigrator.Migration(2026021201, TransactionBehavior.None)]
public sealed class _0001_CreateOrleansMain : FluentMigrator.Migration
{
    public override void Up()
    {
        // Execute.EmbeddedScript("_0001_CreateOrleansMain_Up.sql");
        Execute.EmbeddedScript("SQLServer-Main.sql");
    }

    public override void Down()
    {
        Execute.EmbeddedScript("_0001_CreateOrleansMain_Down.sql");
    }
}
