using FluentMigrator;
using PassKee.Migrations.Code;

namespace PassKee.Migrations.Migrations;

[Migration(3)]
public class _3_UserKdfParams : MyMigration
{
    public override void Up()
    {
        Delete.Column("kdf_params").FromTable("users");

        Create.Table("user_kdf_params")
            .WithColumn("id").AsGuid().PrimaryKey().NotNullable().WithDefault(SystemMethods.NewGuid)
            .WithColumn("user_id").AsGuid().NotNullable().Unique()
            .WithColumn("iterations").AsInt32().NotNullable()
            .WithColumn("memory_size").AsInt32().NotNullable()
            .WithColumn("parallelism").AsInt32().NotNullable()
            .WithColumn("created_at").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
            .WithColumn("updated_at").AsDateTime().Nullable()
            .WithColumn("deleted_at").AsDateTime().Nullable();

        Create.ForeignKey()
            .FromTable("user_kdf_params").ForeignColumn("user_id")
            .ToTable("users").PrimaryColumn("id")
            .OnDelete(System.Data.Rule.Cascade);

        base.Up();
    }

    public override void Down()
    {
        Delete.Table("user_kdf_params");

        Alter.Table("users")
            .AddColumn("kdf_params").AsString(int.MaxValue).Nullable();

        base.Down();
    }
}
