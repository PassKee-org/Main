using Microsoft.Extensions.Logging;
using NHibernate;
using NHibernate.SqlCommand;
using ILogger = Serilog.ILogger;

namespace PassKee.Orm.Connection.Interceptors
{
    public class SqlQueryInterceptor : EmptyInterceptor
    {
        private readonly ILogger<object> _logger;

        public SqlQueryInterceptor(ILogger<object> logger)
        {
            _logger = logger;
        }

        private readonly string[] Exclusions = [
            "SELECT",
            "EXEC"
        ];
        
        public override SqlString OnPrepareStatement(SqlString sql)
        {
            var trimmedSql = sql.Trim();
            var isShouldBeLogged = !Exclusions.Any(item => trimmedSql.StartsWithCaseInsensitive(item));
            if (isShouldBeLogged)
            {
                _logger.LogDebug($"NHibernate: {sql}");
            }
            return base.OnPrepareStatement(sql);
        }
    }
}
