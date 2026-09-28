using FluentMigrator;
using PassKee.Migrations.Code;

namespace PassKee.Migrations.Migrations;

[Migration(6)]
public class _6_VaultEncryptedKey : MyMigration
{
    public override void Up()
    {
        Alter.Table("vaults")
            .AddColumn("encrypted_vault_key").AsBinary().Nullable();
    }

    public override void Down()
    {
        Delete.Column("encrypted_vault_key").FromTable("vaults");
    }
}
