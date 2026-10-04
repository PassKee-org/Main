using System.Data;
using FluentMigrator;
using PassKee.Migrations.Code;

namespace PassKee.Migrations.Migrations;

[Migration(11)]
public class _11_Tags : MyMigration
{
    public override void Up()
    {
        Create.Table("tags")
            .WithColumn("id").AsGuid().PrimaryKey().NotNullable().WithDefault(SystemMethods.NewGuid)
            .WithColumn("vault_id").AsGuid().NotNullable().ForeignKey("fk_tags_vault", "vaults", "id").OnDelete(Rule.Cascade)
            .WithColumn("encrypted_name").AsBinary().NotNullable()
            .WithColumn("created_at").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
            .WithColumn("updated_at").AsDateTime().Nullable()
            .WithColumn("deleted_at").AsDateTime().Nullable();

        Create.Index("idx_tags_vault_id").OnTable("tags").OnColumn("vault_id");

        base.Up();
    }

    public override void Down()
    {
        Delete.Table("tags");

        base.Down();
    }
}
