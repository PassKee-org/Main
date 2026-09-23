using FluentMigrator;
using PassKee.Migrations.Code;

namespace PassKee.Migrations.Migrations;

[Migration(1)]
public class _1_Init : MyMigration
{
    public override void Up()
    {
        Execute.Sql("CREATE EXTENSION IF NOT EXISTS \"uuid-ossp\";");
        Execute.Sql("CREATE EXTENSION IF NOT EXISTS \"pgcrypto\";");

        Create.Table("users")
            .WithColumn("id").AsGuid().PrimaryKey().NotNullable().WithDefault(SystemMethods.NewGuid)
            .WithColumn("email").AsString(255).NotNullable().Unique()
            .WithColumn("created_at").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
            .WithColumn("updated_at").AsDateTime().Nullable()
            .WithColumn("deleted_at").AsDateTime().Nullable();

        Create.Table("queues")
            .WithColumn("id").AsGuid().PrimaryKey().NotNullable().WithDefault(SystemMethods.NewGuid)
            .WithColumn("status").AsInt32().NotNullable()
            .WithColumn("channel").AsInt32().NotNullable()
            .WithColumn("priority").AsInt32().NotNullable()
            .WithColumn("error").AsString(int.MaxValue).Nullable()
            .WithColumn("context_type").AsString(255).NotNullable()
            .WithColumn("context_data").AsString(int.MaxValue).NotNullable()
            .WithColumn("process_at").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
            .WithColumn("created_at").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
            .WithColumn("updated_at").AsDateTime().Nullable()
            .WithColumn("deleted_at").AsDateTime().Nullable();

        Create.Table("sequences")
            .WithColumn("id").AsGuid().PrimaryKey().Unique().NotNullable().WithDefault(SystemMethods.NewGuid)
            .WithColumn("counter").AsInt64().Nullable().WithDefaultValue(1)
            .WithColumn("entity").AsString().Nullable()
            .WithColumn("entity_id").AsString().Nullable()
            .WithColumn("created_at").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
            .WithColumn("updated_at").AsDateTime().Nullable();

        Create.UniqueConstraint().OnTable("sequences").Columns("entity", "entity_id");

        base.Up();
    }

    public override void Down()
    {
        Delete.Table("sequences");
        Delete.Table("queues");
        Delete.Table("users");

        base.Down();
    }
}
