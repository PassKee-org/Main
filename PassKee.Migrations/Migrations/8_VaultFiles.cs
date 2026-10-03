using FluentMigrator;

namespace PassKee.Migrations.Migrations;

[Migration(8)]
public class VaultFiles : Migration
{
    public override void Up()
    {
        Create.Table("vault_file")
            .WithColumn("id").AsCustom("uuid").NotNullable().PrimaryKey()
            .WithColumn("vault_id").AsCustom("uuid").NotNullable()
            .WithColumn("cloud_file_path").AsString(1000).NotNullable()
            .WithColumn("size").AsInt64().NotNullable()
            .WithColumn("created_at").AsDateTime().NotNullable()
            .WithColumn("updated_at").AsDateTime().Nullable()
            .WithColumn("deleted_at").AsDateTime().Nullable();

        Create.Index("idx_vault_file_vault_id").OnTable("vault_file").OnColumn("vault_id");
        
        Create.ForeignKey("fk_vault_file_vault_id")
            .FromTable("vault_file").ForeignColumn("vault_id")
            .ToTable("vaults").PrimaryColumn("id")
            .OnDelete(System.Data.Rule.Cascade);
    }

    public override void Down()
    {
        Delete.Table("vault_file");
    }
}
