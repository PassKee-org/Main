using FluentMigrator;

namespace PassKee.Migrations.Code
{
    public class MyMigration : Migration
    {
        private readonly string _dirScripts = Path.Combine("SQLFiles", "Scripts");
        private readonly string _dirProcedures = Path.Combine("SQLFiles", "Procedures");

        public override void Down()
        {

        }

        public override void Up()
        {
        }

        protected void UpdateProcedure(string Name)
        {
            string? path = System.Reflection.Assembly.GetExecutingAssembly().Location;
            path = System.IO.Path.GetDirectoryName(path);
            path = Path.Combine(path, _dirProcedures) + Path.DirectorySeparatorChar;
            ExecuteScript(Name, path);
        }

        protected void ExecuteScriptByName(string Name)
        {
            ExecuteScriptByName(Name, null);
        }

        protected void ExecuteScriptByName(string Name, string SubPath = null)
        {
            string? path = System.Reflection.Assembly.GetExecutingAssembly().Location;
            path = System.IO.Path.GetDirectoryName(path);
            path = Path.Combine(path, _dirScripts) + Path.DirectorySeparatorChar;
            if (SubPath != null)
            {
                path = Path.Combine(path, SubPath) + Path.DirectorySeparatorChar;
            }
            ExecuteScript(Name, path);
        }

        private void ExecuteScript(string name, string? Path)
        {
            try
            {
                // Only get files that begin with the letter "c."
                var files = Directory.GetFiles(Path, "*.sql")
                                    .Select(fn => new FileInfo(fn))
                                    .Where(file => file.Name.ToLower() == $"{name}.sql".ToLower())
                                    .OrderBy(f => f.Name);
                foreach (FileInfo file in files)
                {
                    var filePath = Path + file.Name;
                    if (!File.Exists(filePath))
                    {
                        throw new Exception($"File not found: {filePath}");
                    }

                    Execute.Script(filePath);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"The process failed: {e.ToString()}");
                throw new Exception($"The process failed: {e.ToString()}");
            }
        }

        protected string GetBaseDirectory()
        {
            string path = System.Reflection.Assembly.GetExecutingAssembly().Location;
            return System.IO.Path.GetDirectoryName(path);
        }
    }
}
