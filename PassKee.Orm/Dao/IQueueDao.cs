using Domain.Abstractions;
using PassKee.Orm.Constants;
using PassKee.Orm.Entities;

namespace PassKee.Orm.Dao;

public interface IQueueDao: IScopedDomainService, IDisposable
{
    Task<QueueEntity?> GetById(
        Guid id,
        CancellationToken cancellationToken = default
    );

    Task Push(
        object context,
        QueueChannel channel = QueueChannel.Default,
        DateTime? processAt = null,
        QueuePriority? priority = null,
        CancellationToken cancellationToken = default
    );

    Task<QueueEntity?> GetTop(QueueChannel? channel = null, CancellationToken cancellationToken = default);

    Task MarkAsProcessed(
        QueueEntity item,
        string? error = null,
        CancellationToken cancellationToken = default
    );

    Task<int> CompleteAllPending(CancellationToken cancellationToken = default);

    Task Flush();

    Task UpdateProcessAtForPending();
    
    void Clear();
}
