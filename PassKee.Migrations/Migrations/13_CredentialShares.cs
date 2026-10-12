using System.Data;
using FluentMigrator;
using PassKee.Migrations.Code;

namespace PassKee.Migrations.Migrations;

[Migration(13)]
public class _13_CredentialShares : MyMigration
{
    public override void Up()
    {
        Create.Table("credential_shares")
            .WithColumn("id").AsGuid().PrimaryKey().NotNullable().WithDefault(SystemMethods.NewGuid)
            .WithColumn("user_id").AsGuid().NotNullable().ForeignKey("fk_credential_shares_user", "users", "id").OnDelete(Rule.Cascade)
            .WithColumn("credential_id").AsGuid().Nullable().ForeignKey("fk_credential_shares_credential", "credentials", "id").OnDelete(Rule.SetNull)
            .WithColumn("token").AsString(128).NotNullable().Unique()
            .WithColumn("type").AsInt32().NotNullable()
            .WithColumn("encrypted_body").AsBinary().NotNullable()
            .WithColumn("expires_at").AsDateTime().Nullable()
            .WithColumn("max_views").AsInt32().Nullable()
            .WithColumn("view_count").AsInt32().NotNullable().WithDefaultValue(0)
            .WithColumn("created_at").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
            .WithColumn("updated_at").AsDateTime().Nullable()
            .WithColumn("deleted_at").AsDateTime().Nullable();

        Create.Index("idx_credential_shares_token").OnTable("credential_shares").OnColumn("token");

        base.Up();
    }

    public override void Down()
    {
        Delete.Table("credential_shares");

        base.Down();
    }
}

