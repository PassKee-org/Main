using FluentMigrator;
using PassKee.Migrations.Code;

namespace PassKee.Migrations.Migrations;

[Migration(14)]
public class _14_CredentialOrder : MyMigration
{
    public override void Up()
    {
        Alter.Table("credentials")
            .AddColumn("order").AsInt32().NotNullable().WithDefaultValue(0);

        base.Up();
    }

    public override void Down()
    {
        Delete.Column("order").FromTable("credentials");

        base.Down();
    }
}

