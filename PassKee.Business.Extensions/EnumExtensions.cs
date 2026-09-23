using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace PassKee.Business.Extensions;

public static class EnumExtensions
{
    public static string GetDisplayName(this Enum enumValue)  
    {  
        var genericEnumType = enumValue.GetType();
        var memberInfo = genericEnumType.GetMember(enumValue.ToString());
        if (memberInfo.Any())
        {
            var attribs = memberInfo[0].GetCustomAttributes(
                typeof(DescriptionAttribute),
                false
            );
            if (attribs.Any())
            {
                return ((DescriptionAttribute)attribs.ElementAt(0)).Description;
            }
            attribs = memberInfo[0].GetCustomAttributes(
                typeof(DisplayAttribute),
                false
            );
            if (attribs.Any())
            {
                return ((DisplayAttribute)attribs.ElementAt(0)).GetName();
            }
        }
        return enumValue.ToString();
    }
}
