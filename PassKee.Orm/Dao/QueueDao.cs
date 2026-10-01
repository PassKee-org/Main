using Microsoft.Extensions.Logging;
using NHibernate;
using NHibernate.Linq;
using NHibernate.Transform;
using Persistence.Transactions.Behaviors;
using PassKee.Business.Common.Helpers;
using PassKee.Orm.Constants;
using PassKee.Orm.Entities;

namespace PassKee.Orm.Dao;

public class QueueDao: IQueueDao
{
    private readonly ILogger<IQueueDao> _logger;
    private readonly ISession _session;

    public QueueDao(IDbSessionProvider sessionProvider, ILogger<IQueueDao> logger)
    {
        _logger = logger;
        _session = sessionProvider.CreateSession(FlushMode.Manual);
    }

    public async Task Push(
        object context,
        QueueChannel channel = QueueChannel.Default,
        DateTime? processAt = null,
        QueuePriority? priority = null,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var contextType = context.GetType();
            var typeString = string.Join(".", contextType.Namespace, contextType.Name);
            var queueItem = new QueueEntity
            {
                Channel = channel,
                Status = QueueStatus.Pending,
                Priority = priority ?? QueuePriority.Normal,
                ContextType = typeString,
                ContextData = JsonHelper.SerializeToString(context),
                ProcessAt = processAt ?? DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _session.SaveAsync(queueItem, cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
        }
    }
    
    public async Task<QueueEntity?> GetById(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        var query = _session.Query<QueueEntity>()
            .Where(item => item.Id == id && item.DeletedAt == null);
        return await query
            .OrderBy(item => item.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
    }
    
    public async Task<QueueEntity?> GetTop(
        QueueChannel? channel = null,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var result = await _session.CreateSQLQuery(@"SELECT * FROM fn_queue_get_top(:channel)")
                .AddEntity(typeof(QueueEntity))
                .SetParameter("channel", channel ?? QueueChannel.Default)
                .SetResultTransformer(new RootEntityResultTransformer())
                .ListAsync<QueueEntity>(cancellationToken);
            var entity = result?.FirstOrDefault();
            if (entity != null)
                await _session.RefreshAsync(entity, cancellationToken);
            return entity;
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
        }
        return null;
    }

    public async Task MarkAsProcessed(
        QueueEntity item,
        string? error = null,
        CancellationToken cancellationToken = default
    )
    {
        if (item.Status != QueueStatus.InProcess)
        {
            throw new Exception("This item already processed");
        }

        if (string.IsNullOrEmpty(error))
        {
            item.Status = QueueStatus.Success;
        }
        else
        {
            item.Error = error;
            item.Status = QueueStatus.Fail;
        }
        await _session.SaveAsync(item, cancellationToken);
    }

    public async Task<int> CompleteAllPending(CancellationToken cancellationToken = default)
    {
        return await _session.Query<QueueEntity>()
            .Where(x => x.DeletedAt == null)
            .UpdateBuilder()
            .Set(x => x.Status, QueueStatus.Success)
            .UpdateAsync(cancellationToken);
    }
    
    public async Task UpdateProcessAtForPending()
    {
        await _session.Query<QueueEntity>()
            .Where(x => x.Status == QueueStatus.Pending && x.DeletedAt == null)
            .UpdateBuilder()
            .Set(x => x.ProcessAt, DateTime.UtcNow.AddSeconds(-1))
            .UpdateAsync();
    }
    
    public void Clear()
    {
        _session.Clear();
    }
    
    public void Dispose()
    {
        Flush().Wait();
        if (_session.IsOpen)
        {
            _session.Dispose();
        }
    }
    
    public async Task Flush()
    {
        await _session.FlushAsync();
    }
}
