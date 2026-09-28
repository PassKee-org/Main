using FluentMigrator;
using PassKee.Migrations.Code;

namespace PassKee.Migrations.Migrations;

[Migration(5)]
public class _5_VaultsAndCredentials : MyMigration
{
    public override void Up()
    {
        Create.Table("vaults")
            .WithColumn("id").AsGuid().PrimaryKey().NotNullable().WithDefault(SystemMethods.NewGuid)
            .WithColumn("user_id").AsGuid().NotNullable().ForeignKey("fk_vaults_user", "users", "id")
            .WithColumn("name").AsString(255).NotNullable()
            .WithColumn("created_at").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
            .WithColumn("updated_at").AsDateTime().Nullable()
            .WithColumn("deleted_at").AsDateTime().Nullable();

        Create.Table("directories")
            .WithColumn("id").AsGuid().PrimaryKey().NotNullable().WithDefault(SystemMethods.NewGuid)
            .WithColumn("vault_id").AsGuid().NotNullable().ForeignKey("fk_directories_vault", "vaults", "id")
            .WithColumn("parent_directory_id").AsGuid().Nullable().ForeignKey("fk_directories_parent", "directories", "id")
            .WithColumn("encrypted_name").AsBinary().NotNullable()
            .WithColumn("created_at").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
            .WithColumn("updated_at").AsDateTime().Nullable()
            .WithColumn("deleted_at").AsDateTime().Nullable();

        Create.Table("credentials")
            .WithColumn("id").AsGuid().PrimaryKey().NotNullable().WithDefault(SystemMethods.NewGuid)
            .WithColumn("vault_id").AsGuid().NotNullable().ForeignKey("fk_credentials_vault", "vaults", "id")
            .WithColumn("directory_id").AsGuid().Nullable().ForeignKey("fk_credentials_directory", "directories", "id")
            .WithColumn("type").AsInt32().NotNullable()
            .WithColumn("encrypted_body").AsBinary().NotNullable()
            .WithColumn("created_at").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
            .WithColumn("updated_at").AsDateTime().Nullable()
            .WithColumn("deleted_at").AsDateTime().Nullable();

        base.Up();
    }

    public override void Down()
    {
        Delete.Table("credentials");
        Delete.Table("directories");
        Delete.Table("vaults");

        base.Down();
    }
}
