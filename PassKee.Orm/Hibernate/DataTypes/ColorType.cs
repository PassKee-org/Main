using System.Data;
using System.Data.Common;
using System.Drawing;
using NHibernate;
using NHibernate.Engine;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;
using PassKee.Business.Extensions;
using PassKee.Orm.Extensions;

namespace PassKee.Orm.Hibernate.DataTypes;

public class ColorType : IUserType
{
    public object Assemble(object cached, object owner)
    {
        return cached;
    }

    public object DeepCopy(object value)
    {
        return value;
    }

    public object Disassemble(object value)
    {
        return value;
    }

    public new bool Equals(object x, object y)
    {
        if (ReferenceEquals(x, y))
        {
            return true;
        }

        if (x == null || y == null)
        {
            return false;
        }

        return x.Equals(y);
    }

    public int GetHashCode(object x)
    {
        return x == null ? typeof(Color).GetHashCode() + 473 : x.GetHashCode();
    }

    public object NullSafeGet(DbDataReader rs, string[] names, ISessionImplementor session, object owner)
    {
        var obj = NHibernateUtil.String.NullSafeGet(rs, names[0], session);
        if (obj == null)
        {
            return null;
        }

        return ColorTranslator.FromHtml((string) obj);
    }
    
    public void NullSafeSet(DbCommand cmd, object value, int index, ISessionImplementor session)
    {
        if (value == null)
        {
            ((IDataParameter) cmd.Parameters[index]).Value = DBNull.Value;
        }
        else
        {
            ((IDataParameter) cmd.Parameters[index]).Value = ((Color) value).ToHexString();
        }
    }

    public bool IsMutable
    {
        get { return true; }
    }

    public object Replace(object original, object target, object owner)
    {
        return original;
    }

    public Type ReturnedType
    {
        get { return typeof(Color); }
    }

    public SqlType[] SqlTypes
    {
        get { return new[] {new SqlType(DbType.StringFixedLength)}; }
    }
}
