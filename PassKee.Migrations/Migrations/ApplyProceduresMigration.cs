using FluentMigrator;
using PassKee.Migrations.Code;

namespace PassKee.Migrations.Migrations;

[Migration(99999999)]
public class ApplyProceduresMigration : MyMigration
{
    private readonly string _dirProcedures = Path.Combine("{0}", "SQLFiles", "Procedures");

    public override void Up()
    {
        UpdateProcedures();
        base.Up();
    }

    public override void Down()
    {

        base.Down();
    }

    protected void UpdateProcedures()
    {
        string path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? AppContext.BaseDirectory;
        path = String.Format(_dirProcedures + Path.DirectorySeparatorChar, path);

        try
        {
            // Only get files that begin with the letter "c."
            var files = Directory.GetFiles(path, "*.sql")
                .Select(fn => new FileInfo(fn))
                .OrderBy(f => f.Name);
            foreach (FileInfo file in files)
            {
                Execute.Script(path + file.Name);
            }
        }
        catch (Exception e)
        {
            var error = $"The process failed: {e.ToString()}";
            Console.WriteLine(error);
            throw new Exception(error);
        }
    }
}
