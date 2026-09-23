using System.Reflection;
using Autofac;
using FluentNHibernate;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Mapping.Providers;
using Microsoft.Extensions.Configuration;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Driver;
using NHibernate.Event;
using NHibernate.Mapping.Attributes;
using Persistence.Transactions.Behaviors;
using Serilog;
using PassKee.Business.Common.Utils;
using PassKee.Orm.Core.Conventions;
using PassKee.Orm.Core.Dialects;
using PassKee.Orm.Core.Interceptors;

namespace PassKee.Orm.Connection
{
    public class DbConnectionFactory : IDbConnectionFactory
    {
        private readonly string _dbNamespace = "PassKee.Orm";
        
        private readonly string _connectionString;
        private readonly IConfiguration _configuration;
        private ISessionFactory? _sessionFactory = null;
        
        private readonly CustomFlushEntityEventListener _customFlushEntityEventListener;
        private readonly EntityPreInsertEventInterceptor _entityPreInsertEventInterceptor;

        public DbConnectionFactory(
            IConfiguration configuration,
            ILifetimeScope scope
        )
        {
            this._configuration = configuration;
            
            _entityPreInsertEventInterceptor = new EntityPreInsertEventInterceptor(scope);
            _customFlushEntityEventListener = new CustomFlushEntityEventListener(scope);
            
            _connectionString = _configuration.GetConnectionString("DefaultConnection")!;
        }

        public Task<ISessionFactory> GetSessionFactoryAsync()
        {
            if (_sessionFactory == null)
            {
                var properties = new Dictionary<string, string>{};
                var isShowSql = _configuration.GetValue<bool>("Hibernate:IsShowSql", false);
                _sessionFactory = BuildFactory(properties, isShowSql);
            }
            return Task.FromResult(_sessionFactory);
        }

        public void Dispose()
        {
            _sessionFactory?.Dispose();
        }

        private ISessionFactory BuildFactory(IDictionary<string, string> properties)
        {
            var currentAssembly = Assembly.GetExecutingAssembly();
            var hibernateConfiguration = new Configuration()
                .AddProperties(properties)
                .Configure(currentAssembly, $"{_dbNamespace}.Hibernate.hibernate.hbm.xml");

            // Enable validation (optional)
            HbmSerializer.Default.Validate = true;

            // Import all entities and queries
            hibernateConfiguration.AddInputStream(HbmSerializer.Default.Serialize(currentAssembly));
            // Import all mapping files
            var classes = currentAssembly.GetManifestResourceNames()
                .Where(resourceName => resourceName.StartsWith($"{_dbNamespace}.Queries"));
            foreach (var resourceName in classes)
            {
                var filePath = IoUtils.GetResourcePath(currentAssembly, resourceName);
                hibernateConfiguration.AddInputStream(currentAssembly.GetManifestResourceStream(filePath));
            }
            return hibernateConfiguration.BuildSessionFactory();
        }
        
        private ISessionFactory BuildFactory(IDictionary<string, string> properties, bool isShowSql)
        {
            // Logging
            var currentAssembly = Assembly.GetExecutingAssembly();

            var assemblyDirectory = Path.GetDirectoryName(currentAssembly?.Location)!;
            var msSqlHbmTextWriter = new StringWriter();
            var postgresHbmTextWriter = new StringWriter();
            var sessionBuilder = Fluently.Configure()
                .Mappings(m =>
                { 
                    m.FluentMappings.Conventions.Add<EnumConvention>();
                    m.FluentMappings.Conventions.Add<ExcludeMappingsConvention>();
                    m.FluentMappings.Conventions.Add<SnakeCaseConvention>();
                    m.FluentMappings.ExportTo(postgresHbmTextWriter);
                    m.UsePersistenceModel(GetPersistenceModel());
                })
                .Cache(cacheBuilder =>
                {
                    // cacheBuilder.UseQueryCache();
                    // cacheBuilder.UseSecondLevelCache();
                    // cacheBuilder.ProviderClass<HashtableCacheProvider>();
                })
                .ExposeConfiguration(config =>
                {
                    // config.DataBaseIntegration(db =>
                    // {
                    //     db.SchemaAction = SchemaAutoAction.Validate;        
                    // });
                    
                    config.AddProperties(properties);
                    config.Properties.Add("query.substitutions", "true=1, false=0");
                    config.Properties.Add("hbm2ddl.keywords", "auto-quote");
                    config.Properties.Add("adonet.batch_size", "20");
                    
                    // config.AddResource($"{_dbNamespace}.hibernate.hbm.xml", currentAssembly);

                    // try
                    // {
                    //     // Find and try generate mapping file with native mapping configuration
                    //     // Import all entities and queries
                    //     // var serializedHibernateConfigStream = HbmSerializer.Default.Serialize(currentAssembly);
                    //     // using var serializedHibernateConfigStreamReader = new StreamReader(serializedHibernateConfigStream);
                    //     // var hibernateConfigXml = serializedHibernateConfigStreamReader.ReadToEnd();
                    //     // // hibernateConfiguration.AddXmlString(hibernateConfigXml);
                    //     // config.AddXmlString(hibernateConfigXml);
                    // }
                    // catch (Exception e)
                    // {
                    //     Log.Logger.Information($"Default NHibernate mapping information: {e.Message}");
                    // }

                    // Import all mapping files
                    var classes = currentAssembly!.GetManifestResourceNames()
                        .Where(resourceName => resourceName.StartsWith($"{_dbNamespace}.Hibernate.Queries"));
                    foreach (var resourceName in classes)
                    {
                        var filePath = IoUtils.GetResourcePath(currentAssembly, resourceName);
                        config.AddInputStream(currentAssembly.GetManifestResourceStream(filePath));
                    }

                    // Init interceptors
                    config.EventListeners.FlushEntityEventListeners = [ _customFlushEntityEventListener ];
                    var insertInterceptors = new IPreInsertEventListener[]
                    {
                        _entityPreInsertEventInterceptor
                    };
                    config.AppendListeners(ListenerType.PreInsert, insertInterceptors);
                    
                    // Validate schema
                    // new SchemaValidator(config).Validate();
                    
                    // Export schema
                    // var schemaExporter = new SchemaExport(config);
                    // schemaExporter.SetOutputFile(Path.Combine(assemblyDirectory, "db_schema.sql"))
                    //     .Create(false, false);
                });
            var databaseConfig = PostgreSQLConfiguration.PostgreSQL83
                .Dialect<CustomPostgresSqlDialect>()
                .ConnectionString(_connectionString);
            if (isShowSql)
                databaseConfig = databaseConfig.ShowSql().FormatSql();
            sessionBuilder = sessionBuilder.Database(databaseConfig);
            File.WriteAllText(Path.Combine(assemblyDirectory, "nhibenate_mappings_mssql.xml"), msSqlHbmTextWriter.ToString());
            File.WriteAllText(Path.Combine(assemblyDirectory, "nhibenate_mappings_postgres.xml"), msSqlHbmTextWriter.ToString());
            return sessionBuilder.BuildSessionFactory();
        }

        private PersistenceModel GetPersistenceModel()
        {
            var assembly = typeof(BusinessOrmAssemblyMarker).Assembly;
            var filteredTypes = assembly.GetTypes()
                .Where(t => (
                                typeof(IMappingProvider).IsAssignableFrom(t) 
                                || typeof(IExternalComponentMappingProvider).IsAssignableFrom(t)
                                || typeof(IComponentMappingProvider).IsAssignableFrom(t)
                                || typeof(IIndeterminateSubclassMappingProvider).IsAssignableFrom(t)
                            )
                            && !t.IsAbstract 
                            && !ExcludeMappingsConvention.ExcludedMappings.Any(item => item.IsAssignableFrom(t))
                            && t.Namespace != null
                            && t.Namespace.StartsWith(_dbNamespace)
                )
                .ToList();
            
            var persistenceModel = new PersistenceModel();
            persistenceModel.Conventions.Add<ExcludeMappingsConvention>();
            foreach (var type in filteredTypes)
            {
                persistenceModel.Add(type);
            }
            return persistenceModel;
        }
    }
}
