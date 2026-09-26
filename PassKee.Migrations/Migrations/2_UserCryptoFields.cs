using FluentMigrator;
using PassKee.Migrations.Code;

namespace PassKee.Migrations.Migrations;

[Migration(2)]
public class _2_UserCryptoFields : MyMigration
{
    public override void Up()
    {
        Alter.Table("users")
            .AddColumn("auth_salt").AsBinary().Nullable()
            .AddColumn("server_hash").AsBinary().Nullable()
            .AddColumn("kdf_params").AsString(int.MaxValue).Nullable()
            .AddColumn("user_public_key").AsBinary().Nullable()
            .AddColumn("encrypted_user_private_key").AsBinary().Nullable()
            .AddColumn("encrypted_user_vault_key").AsBinary().Nullable();
    }

    public override void Down()
    {
        Delete.Column("auth_salt").FromTable("users");
        Delete.Column("server_hash").FromTable("users");
        Delete.Column("kdf_params").FromTable("users");
        Delete.Column("user_public_key").FromTable("users");
        Delete.Column("encrypted_user_private_key").FromTable("users");
        Delete.Column("encrypted_user_vault_key").FromTable("users");
    }
}
