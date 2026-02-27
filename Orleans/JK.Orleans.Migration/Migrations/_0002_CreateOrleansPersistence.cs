using FluentMigrator;

namespace JK.Orleans.Migration.Migrations;

[FluentMigrator.Migration(2026021202, "Create Orleans persistence (grain storage) table")] 
public sealed class _0002_CreateOrleansPersistence : FluentMigrator.Migration
{
    public override void Up()
    {
        // Execute.EmbeddedScript("_0002_CreateOrleansPersistence_Up.sql");
        Execute.EmbeddedScript("SQLServer-Clustering.sql");
    }

    public override void Down()
    {
        Execute.EmbeddedScript("_0002_CreateOrleansPersistence_Down.sql");
    }

}
