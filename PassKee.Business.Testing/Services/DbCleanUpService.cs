using System.Collections.Generic;
using System.Threading.Tasks;
using Persistence.Transactions.Behaviors;

namespace PassKee.Business.Testing.Services;

public class DbCleanUpService : IDbCleanUpService
{
    private readonly IDbSessionProvider _sessionProvider;

    public DbCleanUpService(IDbSessionProvider sessionProvider)
    {
        _sessionProvider = sessionProvider;
    }

    public async Task CleanUp()
    {
        var tables = new List<string>
        {
            "user_kdf_params",
            "queues",
            "users",
        };

        foreach (var table in tables)
        {
            await _sessionProvider.CurrentSession.CreateSQLQuery($"delete from {table} where 1=1;").ExecuteUpdateAsync();    
        }
        _sessionProvider.CurrentSession.Clear();
    }
}
