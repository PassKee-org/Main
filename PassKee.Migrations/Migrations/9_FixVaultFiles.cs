using FluentMigrator;

namespace PassKee.Migrations.Migrations;

[Migration(9)]
public class FixVaultFiles : Migration
{
    public override void Up()
    {
        if (Schema.Table("vault_file").Column("is_deleted").Exists())
        {
            Delete.Column("is_deleted").FromTable("vault_file");
        }
        if (Schema.Table("vault_file").Column("is_new").Exists())
        {
            Delete.Column("is_new").FromTable("vault_file");
        }
    }

    public override void Down()
    {
    }
}
