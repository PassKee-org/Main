using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace PassKee.Business.Extensions
{
    public static class ObjectExtensions
    {
        public static bool EqualsToTypeName(this Object value, string typeName)
        {
            return value.GetType().Name.ToLower().Equals(typeName?.ToLower());
        }
        
        public static string GetAsJson(this Object value)
        {
            try
            {
                return JsonConvert.SerializeObject(value);
            }
            catch (Exception)
            {
                return String.Empty;
            }
        }
        
        public static string GetDisplayName(this Type genericEnumType, object enumValue)  
        {
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
}
