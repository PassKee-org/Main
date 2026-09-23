using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.AcceptanceCriteria;
using FluentNHibernate.Conventions.Inspections;
using FluentNHibernate.Conventions.Instances;

namespace PassKee.Orm.Core.Conventions;

public class EnumConvention : IUserTypeConvention
{
    public void Accept(IAcceptanceCriteria<IPropertyInspector> criteria)
    {
        criteria.Expect(x => x.Property.PropertyType.IsEnum ||
                             (x.Property.PropertyType.IsGenericType && 
                              x.Property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) &&
                              x.Property.PropertyType.GetGenericArguments()[0].IsEnum)
        );
    }

    public void Apply(IPropertyInstance target)
    {
        if (target.Property.PropertyType.IsEnum)
        {
            target.CustomType(target.Property.PropertyType);
        }
    }
}
