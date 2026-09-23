using AutoMapper;

namespace PassKee.Business.Extensions;

public static class AutoMapperExtensions
{
    public static IMappingExpression<TSource, TDestination> IgnoreAllUnmapped<TSource, TDestination>(this IMappingExpression<TSource, TDestination> expression)
    {
        expression.ForAllMembers(opt => opt.Ignore());
        return expression;
    }
    
    /// <summary>
    /// Supply a custom instantiation function for the destination type, based on the entire resolution context
    /// </summary>
    /// <remarks>Not used for LINQ projection (ProjectTo)</remarks>
    /// <param name="ctor">Callback to create the destination type given the current resolution context</param>
    /// <returns>Itself</returns>
    public static IMappingExpression<TSource, TDestination> IgnoreAllAndConstructUsing<TSource, TDestination>(this IMappingExpression<TSource, TDestination> expression, Func<TSource, ResolutionContext, TDestination> ctor)
    {
        expression.ForAllMembers(opt => opt.Ignore());
        expression.ConstructUsing(ctor);
        return expression;
    }
}
