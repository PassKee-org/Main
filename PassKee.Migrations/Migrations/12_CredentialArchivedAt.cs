using FluentMigrator;
using PassKee.Migrations.Code;

namespace PassKee.Migrations.Migrations;

[Migration(12)]
public class _12_CredentialArchivedAt : MyMigration
{
    public override void Up()
    {
        Alter.Table("credentials")
            .AddColumn("archived_at").AsDateTime().Nullable();

        base.Up();
    }

    public override void Down()
    {
        Delete.Column("archived_at").FromTable("credentials");

        base.Down();
    }
}

