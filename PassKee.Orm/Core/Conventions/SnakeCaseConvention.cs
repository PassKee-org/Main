using System.Text.RegularExpressions;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.AcceptanceCriteria;
using FluentNHibernate.Conventions.Inspections;
using FluentNHibernate.Conventions.Instances;

namespace PassKee.Orm.Core.Conventions;

public class SnakeCaseConvention : 
    IClassConvention,
    IPropertyConvention,
    IPropertyConventionAcceptance,
    IReferenceConvention,
    IHasManyConvention,
    IHasOneConvention,
    IReferenceConventionAcceptance

{
    public void Apply(IClassInstance instance)
    {
        var snakeCasedTableName = instance.EntityType.Name
            .Replace("Entity", "");
        instance.Table(ToSnakeCase(snakeCasedTableName));
    }
    
    public void Accept(IAcceptanceCriteria<IPropertyInspector> criteria)
    {
        criteria.Expect(x => string.IsNullOrEmpty(x.Formula)); // Пропускаем свойства с формулой
    }
    
    public void Accept(IAcceptanceCriteria<IManyToOneInspector> criteria)
    {
        criteria.Expect(x => string.IsNullOrEmpty(x.Formula));
    }
    
    public void Apply(IPropertyInstance instance)
    {
        instance.Column(ToSnakeCase(instance.Name));
    }    
    

    public void Apply(IManyToOneInstance instance)
    {
        var columnName = ToSnakeCase(instance.Name);
        if (!columnName.EndsWith("Id"))
        {
            columnName += "_id";
        }
        instance.Column(columnName);
    }

    public void Apply(IOneToManyCollectionInstance instance)
    {
        // instance.Table(ToSnakeCase(instance.TableName));
        // instance.Key.Column(ToSnakeCase(instance.Key.Columns.First().Name));
    }

    public void Apply(IOneToOneInstance instance)
    {
    }
    
    private string ToSnakeCase(string name)
    {
        var result = Regex.Replace(name, @"(?<=[a-z0-9])([A-Z])", "_$1");
        result = Regex.Replace(result, @"(?<=[A-Za-z])([0-9])", "_$1");
        result = Regex.Replace(result, @"(?<=[0-9])([A-Za-z])", "_$1");
        return result.ToLower();
    }
}
