using Autofac;
using Persistence.Transactions.Behaviors;
using PassKee.Orm.Constants;
using PassKee.Business.Services.Queue;
using PassKee.WorkerServices.Core;

namespace PassKee.WorkerServices.Services.Queue
{
    internal class DefaultProcessingHostedService : ABackgroundService
    {
        private readonly IQueueService _queueService;

        public DefaultProcessingHostedService() : base()
        {
            _queueService = DiScope.Resolve<IQueueService>();
            ServiceName = "DefaultProcessingHostedService";
        }

        protected override async Task DoWorkAsync(CancellationToken cancellationToken)
        {
            Log($"Worker started at: {DateTime.Now}");
            while (!cancellationToken.IsCancellationRequested)
            {
                await _queueService.ProcessAsync(
                    QueueChannel.Default,
                    cancellationToken
                );
                await DbSessionProvider.PerformCommitAsync(true, cancellationToken);
                DbSessionProvider.CurrentSession.Clear();
                await Task.Delay(1000, cancellationToken);
            }
        }
        
        protected override string GetCrontabExpression() => "* * * * *";
    }
}
