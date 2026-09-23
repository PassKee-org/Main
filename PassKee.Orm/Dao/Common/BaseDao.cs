using System.Collections.Concurrent;
using System.Reflection;
using Autofac;
using Domain.Abstractions.Api;
using NHibernate;
using Persistence.Transactions.Behaviors;

namespace PassKee.Orm.Dao.Common;

public abstract class BaseDao
{
    private static readonly string QueryPathTemplate = "Queries.Sql.{0}.sql";
    private static readonly ConcurrentDictionary<string, string> QueryCache = new();
    
    private readonly IDbSessionProvider _dbSessionProvider;
    protected readonly IBaseApiRequestService? _apiRequestService;

    protected ISession Session => _dbSessionProvider.CurrentSession;
    
    protected BaseDao(ILifetimeScope scope)
    {
        _dbSessionProvider = scope.Resolve<IDbSessionProvider>();
        scope.TryResolve(out IBaseApiRequestService? apiRequestService);
        _apiRequestService = apiRequestService;
    }
    
    private static readonly Assembly CurrentAssembly = Assembly.GetExecutingAssembly();
    
    protected static string ReadSqlQuery(string resourcePath)
    {
        var queryPath = string.Format(QueryPathTemplate, resourcePath);
        return QueryCache.GetOrAdd(resourcePath, ReadResource(queryPath));
    }
    
    private static string ReadResource(string resourcePath)
    {
        var fullName = CurrentAssembly.GetManifestResourceNames()
            .FirstOrDefault(x => x.EndsWith(resourcePath));

        if (fullName == null)
            throw new Exception($"Resource not found: {resourcePath}");

        using var stream = CurrentAssembly.GetManifestResourceStream(fullName);
        ArgumentNullException.ThrowIfNull(stream, $"Resource stream is null: {resourcePath}");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
