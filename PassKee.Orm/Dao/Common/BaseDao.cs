using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Domain.Abstractions.Api;
using NHibernate;
using PassKee.Orm.Core;
using Persistence.Transactions.Behaviors;

namespace PassKee.Orm.Dao.Common;

public abstract class BaseDao : IBaseDao
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

    public virtual async Task DeleteAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : AEntity
    {
        entity.DeletedAt = DateTime.UtcNow;
        await Session.UpdateAsync(entity, cancellationToken);
    }

    public virtual async Task DeleteAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default) where TEntity : AEntity
    {
        var now = DateTime.UtcNow;
        foreach (var entity in entities)
        {
            entity.DeletedAt = now;
            await Session.UpdateAsync(entity, cancellationToken);
        }
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
