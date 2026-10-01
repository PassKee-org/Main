using System.Data;
using FluentMigrator;
using PassKee.Migrations.Code;

namespace PassKee.Migrations.Migrations;

[Migration(7)]
public class _7_DirectoryCascadeDelete : MyMigration
{
    public override void Up()
    {
        Delete.ForeignKey("fk_directories_parent").OnTable("directories");
        Create.ForeignKey("fk_directories_parent")
            .FromTable("directories").ForeignColumn("parent_directory_id")
            .ToTable("directories").PrimaryColumn("id")
            .OnDelete(Rule.Cascade);

        Delete.ForeignKey("fk_credentials_directory").OnTable("credentials");
        Create.ForeignKey("fk_credentials_directory")
            .FromTable("credentials").ForeignColumn("directory_id")
            .ToTable("directories").PrimaryColumn("id")
            .OnDelete(Rule.Cascade);

        base.Up();
    }

    public override void Down()
    {
        Delete.ForeignKey("fk_credentials_directory").OnTable("credentials");
        Create.ForeignKey("fk_credentials_directory")
            .FromTable("credentials").ForeignColumn("directory_id")
            .ToTable("directories").PrimaryColumn("id");

        Delete.ForeignKey("fk_directories_parent").OnTable("directories");
        Create.ForeignKey("fk_directories_parent")
            .FromTable("directories").ForeignColumn("parent_directory_id")
            .ToTable("directories").PrimaryColumn("id");

        base.Down();
    }
}
