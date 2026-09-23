using System.Reflection;
using Autofac;
using Domain.Abstractions;
using Microsoft.Extensions.Logging;
using Notification.Abstractions;
using Persistence.Transactions.Behaviors;
using PassKee.Business.Common.Helpers;
using PassKee.Orm.Constants;
using PassKee.Orm.Dao;
using PassKee.Orm.Entities;

namespace PassKee.Business.Services.Queue;

public partial class QueueService: IQueueService
{
    private readonly IQueueDao _queueDao;
    private readonly ILogger<QueueService> _logger;
    private readonly ILifetimeScope _scope;
    private readonly IDbSessionProvider _dbSessionProvider;
    private readonly IEnumerable<Type> _queueItemAssemblyTypes;

    public QueueService(
        IQueueDao queueDao,
        ILogger<QueueService> logger,
        ILifetimeScope scope,
        IDbSessionProvider dbSessionProvider
    )
    {
        _queueDao = queueDao;
        _logger = logger;
        _scope = scope;
        _dbSessionProvider = dbSessionProvider;
        
        _queueItemAssemblyTypes = AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(a => a.GetTypes());
    }

    public async Task PushDefaultAsync(IQueueItemContext itemContext)
    {
        await _queueDao.Push(itemContext, QueueChannel.Default);
    }
    
    public async Task PushNotificationAsync(INotificationItemContext itemContext)
    {
        await _queueDao.Push(itemContext, QueueChannel.Notifications);
    }
    
    public async Task PushExternalClientAsync(IExternalServiceItemContext itemContext)
    {
        await _queueDao.Push(itemContext, QueueChannel.ExternalClient);
    }
    
    public async Task<int> ProcessAsync(
        QueueChannel channel,
        CancellationToken cancellationToken = default,
        bool isClearSessionForEachIteration = true
    )
    {
        var processedCounter = 0;
        while (true)
        {
            if (isClearSessionForEachIteration)
            {
                _dbSessionProvider.CurrentSession.Clear();
            }
            
            var queueItem = await _queueDao.GetTop(channel, cancellationToken);
            if (queueItem == null)
            {
                break;
            }

            string error = null;
            try
            {
                await HandleQueueItem(queueItem, cancellationToken);
                await _dbSessionProvider.PerformCommitAsync(false, cancellationToken);
                processedCounter++;
            }
            catch (Exception e)
            {
                error = e.Message;
                _logger.LogError(e, e.Message);
            }
            await _queueDao.MarkAsProcessed(queueItem, error: error, cancellationToken: cancellationToken);
        }

        return processedCounter;
    }
    
    private async Task HandleQueueItem(QueueEntity queueEntity, CancellationToken cancellationToken = default)
    {
        var queueItemContextType = _queueItemAssemblyTypes.FirstOrDefault(t => t.FullName == queueEntity.ContextType);
        
        if (queueItemContextType == null)
            throw new InvalidOperationException($"Queue item context type {queueEntity.ContextType} not found");

        Type handlerType = typeof(IAsyncQueueHandler<>).MakeGenericType(queueItemContextType);

        var contextObject = JsonHelper.DeserializeObject(queueEntity.ContextData, queueItemContextType);
        if (contextObject == null)
        {
            _logger.LogError("Notification context parsing error: {Type}", queueEntity.ContextType);
            return;
        }
        
        var handlerInstance = _scope.ResolveOptional(handlerType);
        if (handlerInstance == null)
            throw new InvalidOperationException($"Queue item context handler {queueEntity.ContextType} not found");
        if (contextObject is IQueueItemContext queueItemContext)
        {
            var handlerMethod = handlerType.GetMethod("HandleAsync", [queueItemContextType, typeof(CancellationToken)]);
            if (handlerMethod == null)
                throw new InvalidOperationException($"Method HandleAsync not found for {handlerType.FullName}");

            try
            {
                await (Task)handlerMethod!.Invoke(handlerInstance, [queueItemContext, cancellationToken])!;
            }
            catch (TargetInvocationException tie)
            {
                // Unbox async/await wrapped exception to original exception
                if (tie.InnerException != null)
                {
                    throw tie.InnerException;    
                }
                throw;
            }
            catch (Exception)
            {
                throw;
            }
        }
        else
        {
            throw new Exception($"Incorrect context type for Queue item: {queueEntity.Id}");
        }

        await Task.CompletedTask;
    }
}
