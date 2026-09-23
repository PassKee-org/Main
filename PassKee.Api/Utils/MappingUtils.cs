using AutoMapper;
using PassKee.Business.Common.Extensions;

namespace PassKee.Api.Utils;

public static class MappingUtils
{
    public static TResult BuildWithBase<TSource, TBase, TResult>(
        TSource source,
        IRuntimeMapper mapper,
        Func<TSource, TResult> resultFactory)
        where TBase : class
        where TResult : class
    {
        if (source is null) throw new ArgumentNullException(nameof(source));
        if (mapper is null) throw new ArgumentNullException(nameof(mapper));
        if (resultFactory is null) throw new ArgumentNullException(nameof(resultFactory));

        var baseDto = mapper.Map<TBase>(source);
        var result = resultFactory(source);

        return result.CloneExcept(baseDto);
    }
}
