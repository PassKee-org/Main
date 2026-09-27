using FluentMigrator;
using PassKee.Migrations.Code;

namespace PassKee.Migrations.Migrations;

[Migration(4)]
public class _4_UserTokens : MyMigration
{
    public override void Up()
    {
        Create.Table("user_access_tokens")
            .WithColumn("id").AsGuid().PrimaryKey().NotNullable().WithDefault(SystemMethods.NewGuid)
            .WithColumn("user_id").AsGuid().NotNullable().ForeignKey("users", "id")
            .WithColumn("token").AsString(200).NotNullable()
            .WithColumn("expiration_time").AsDateTime().NotNullable()
            .WithColumn("created_at").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
            .WithColumn("updated_at").AsDateTime().Nullable()
            .WithColumn("deleted_at").AsDateTime().Nullable();

        Create.Index()
            .OnTable("user_access_tokens")
            .OnColumn("token");

        Create.Table("user_jwt_tokens")
            .WithColumn("id").AsGuid().PrimaryKey().NotNullable().WithDefault(SystemMethods.NewGuid)
            .WithColumn("access_token_id").AsGuid().NotNullable().ForeignKey("user_access_tokens", "id")
            .WithColumn("token").AsString(2056).NotNullable()
            .WithColumn("expiration_time").AsDateTime().NotNullable()
            .WithColumn("created_at").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
            .WithColumn("updated_at").AsDateTime().Nullable()
            .WithColumn("deleted_at").AsDateTime().Nullable();

        Create.Index()
            .OnTable("user_jwt_tokens")
            .OnColumn("access_token_id")
            .Ascending()
            .OnColumn("expiration_time")
            .Ascending();

        base.Up();
    }

    public override void Down()
    {
        Delete.Table("user_jwt_tokens");
        Delete.Table("user_access_tokens");

        base.Down();
    }
}
