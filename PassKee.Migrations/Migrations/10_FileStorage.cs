using FluentMigrator;

namespace PassKee.Migrations.Migrations;

/// <summary>
/// Replaces the vault-only "vault_file" table with a common "file_storage" table
/// and a "vault_file_storage" table holding vault-specific data (joined-subclass inheritance).
/// </summary>
[Migration(10)]
public class FileStorage : Migration
{
    public override void Up()
    {
        Create.Table("file_storage")
            .WithColumn("id").AsCustom("uuid").NotNullable().PrimaryKey()
            .WithColumn("cloud_file_path").AsString(1000).NotNullable()
            .WithColumn("size").AsInt64().NotNullable()
            .WithColumn("created_at").AsDateTime().NotNullable()
            .WithColumn("updated_at").AsDateTime().Nullable()
            .WithColumn("deleted_at").AsDateTime().Nullable();

        Create.Table("vault_file_storage")
            .WithColumn("id").AsCustom("uuid").NotNullable().PrimaryKey()
            .WithColumn("vault_id").AsCustom("uuid").NotNullable();

        Execute.Sql(@"
            INSERT INTO file_storage (id, cloud_file_path, size, created_at, updated_at, deleted_at)
            SELECT id, cloud_file_path, size, created_at, updated_at, deleted_at FROM vault_file;
            INSERT INTO vault_file_storage (id, vault_id)
            SELECT id, vault_id FROM vault_file;");

        Create.ForeignKey("fk_vault_file_storage_id")
            .FromTable("vault_file_storage").ForeignColumn("id")
            .ToTable("file_storage").PrimaryColumn("id")
            .OnDelete(System.Data.Rule.Cascade);

        Create.ForeignKey("fk_vault_file_storage_vault_id")
            .FromTable("vault_file_storage").ForeignColumn("vault_id")
            .ToTable("vaults").PrimaryColumn("id")
            .OnDelete(System.Data.Rule.Cascade);

        Create.Index("idx_vault_file_storage_vault_id").OnTable("vault_file_storage").OnColumn("vault_id");

        Delete.Table("vault_file");
    }

    public override void Down()
    {
        Create.Table("vault_file")
            .WithColumn("id").AsCustom("uuid").NotNullable().PrimaryKey()
            .WithColumn("vault_id").AsCustom("uuid").NotNullable()
            .WithColumn("cloud_file_path").AsString(1000).NotNullable()
            .WithColumn("size").AsInt64().NotNullable()
            .WithColumn("created_at").AsDateTime().NotNullable()
            .WithColumn("updated_at").AsDateTime().Nullable()
            .WithColumn("deleted_at").AsDateTime().Nullable();

        Execute.Sql(@"
            INSERT INTO vault_file (id, vault_id, cloud_file_path, size, created_at, updated_at, deleted_at)
            SELECT f.id, v.vault_id, f.cloud_file_path, f.size, f.created_at, f.updated_at, f.deleted_at
            FROM file_storage f JOIN vault_file_storage v ON v.id = f.id;");

        Create.Index("idx_vault_file_vault_id").OnTable("vault_file").OnColumn("vault_id");
        Create.ForeignKey("fk_vault_file_vault_id")
            .FromTable("vault_file").ForeignColumn("vault_id")
            .ToTable("vaults").PrimaryColumn("id")
            .OnDelete(System.Data.Rule.Cascade);

        Delete.Table("vault_file_storage");
        Delete.Table("file_storage");
    }
}
