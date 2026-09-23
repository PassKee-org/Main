using System.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NHibernate;
using Persistence.Transactions.Behaviors;
using PassKee.Orm.Connection.Interceptors;

namespace PassKee.Orm.Connection
{
    public class DbSessionProvider : IDbSessionProvider
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;
        private readonly ILogger<IDbConnectionFactory> _logger;
        private readonly IConfiguration _configuration;
        private IsolationLevel? _transactionalModeIsolationLevel;

        private ISession? _session { get; set; }
        
        private ISessionFactory _sessionFactory { get; set; }

        private bool _isShowSql { get; }

        public ISessionFactory SessionFactory => _sessionFactory;
        
        public ISession CurrentSession {
            get {
                OpenCurrentSession();
                ArgumentNullException.ThrowIfNull(_session);
                return _session;
            }
        }

        private ITransaction? _transaction;

        public DbSessionProvider(
            IDbConnectionFactory dbConnectionFactory, 
            ILogger<IDbConnectionFactory> logger,
            IConfiguration configuration
        )
        {
            _dbConnectionFactory = dbConnectionFactory ?? throw new ArgumentNullException(nameof(dbConnectionFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration;
            _sessionFactory = _dbConnectionFactory.GetSessionFactoryAsync().Result;
            _isShowSql = _configuration.GetValue<bool>("Hibernate:IsShowSql", false);
        }

        ~DbSessionProvider()
        {
            //Dispose();
        }

        public void SetTransactional(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            _transactionalModeIsolationLevel = isolationLevel;
        }
        
        public async Task UnsetTransactional()
        {
            if (_transactionalModeIsolationLevel != null)
            {
                if (_transaction is { IsActive: true })
                {
                    try
                    {
                        await _transaction.CommitAsync();
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e.Message, e);
                        await _transaction.RollbackAsync();
                        throw e;
                    }    
                }
                _transactionalModeIsolationLevel = null;
                _transaction?.Dispose();
            }
        }
        
        public ITransaction BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            OpenCurrentSession();
            return CurrentSession.BeginTransaction(isolationLevel);
        }
        
        public async Task PerformCommitAsync(bool isCloseConnection = true, CancellationToken cancellationToken = default)
        {
            if (_session is null)
                return;
                
            if (_session is { IsOpen: true })
            {
                if (_transaction is { IsActive: true })
                {
                    try
                    {
                        await _transaction.CommitAsync(cancellationToken).WaitAsync(cancellationToken);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e.Message, e);
                        await _transaction.RollbackAsync(cancellationToken).WaitAsync(cancellationToken);
                        throw e;
                    }    
                }
                else
                {
                    await _session.FlushAsync(cancellationToken);
                }
            }
            if (isCloseConnection)
            {
                _transaction?.Dispose();
                if (_session.IsOpen)
                {
                    _session?.Close();
                }
                _transactionalModeIsolationLevel = null;
            }
        }

        public Task RollbackCommitAsync(CancellationToken cancellationToken = default)
        {
            CloseCurrentSession();
            return Task.CompletedTask;
        }
        
        public void OpenCurrentSession()
        {
            if (_session is not {IsOpen: true})
            {
                _session = CreateSession();
            }

            if (_transactionalModeIsolationLevel != null && _transaction is not {IsActive: true})
            {
                _transaction?.Dispose();
                _transaction = _session.BeginTransaction(_transactionalModeIsolationLevel.Value);
            }
        }
        
        public ISession CreateSession(FlushMode? flushMode = null)
        {
            if (_isShowSql)
            {
                var session = _sessionFactory.WithOptions()
                    .Interceptor(
                        new SqlQueryInterceptor(_logger)
                    );
                if (flushMode != null)
                    session = session.FlushMode(flushMode.Value);
                return session.OpenSession();
            }
            return _sessionFactory.OpenSession();
        }
        
        public void CloseCurrentSession()
        {
            if (_session is null || _session is not {IsOpen: true})
                return;
            _transaction?.Dispose();
            _session?.Close();
            _transactionalModeIsolationLevel = null;
        }
        
        #region IDisposable implementation
        public void Dispose()
        {
            CloseCurrentSession();
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
