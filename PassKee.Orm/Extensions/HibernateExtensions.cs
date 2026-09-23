using System.Collections;
using NHibernate;
using NHibernate.Collection;

namespace PassKee.Orm.Extensions
{
    public static class HibernateExtensions
    {
        public static bool IsHibernateLazy(this ICollection collection)
        {
            if (collection is ILazyInitializedCollection)
            {
                return true;
            }
            return false;
        }
        
        public static bool IsHibernateLazy<T>(this ICollection<T> collection)
        {
            if (collection is ILazyInitializedCollection)
            {
                return true;
            }
            return false;
        }
        
        public static T? GetInstanceFromCache<T>(this ISession session, object key) where T : class
        {
            var entity = session.GetSessionImplementation()
                .PersistenceContext
                .EntitiesByKey
                .SingleOrDefault(x => x.Value is T && x.Key.Identifier == key);
            return entity as T;
        }
    }
}
