using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Newtonsoft.Json;

namespace PassKee.Business.Common.Extensions
{
    public static class ObjectExtensions
    {
        public static string GetTypeName(this Object value)
        {
            return value.GetType().Name;
        }
        
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
        
        public static string? GetDisplayName(this Type genericEnumType, object enumValue)
        {
            var memberName = enumValue.ToString();
            if (string.IsNullOrEmpty(enumValue.ToString()))
            {
                return null;
            }

            var memberInfo = genericEnumType.GetMember(memberName ?? string.Empty);
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

        public static T CloneExcept<T, S>(this T target, S source, ICollection<string>? propertyNames = null)
        {
            propertyNames ??= [];
            if (source == null)
            {
                return target;
            }

            var sourceType = typeof(S);
            var targetType = typeof(T);
            var allowedFlags = BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance;
            var allowedFlagsForChildClass = BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;

            var properties = sourceType.GetProperties();
            foreach (PropertyInfo sPI in properties)
            {
                if (!propertyNames.Contains(sPI.Name))
                {
                    PropertyInfo? tPI = null;
                    try
                    {
                        tPI = targetType.GetProperty(sPI.Name, allowedFlags);
                    }
                    catch (AmbiguousMatchException)
                    {
                        // In case when property was re-declared with different type in child class
                    }
                    if (tPI == null)
                    {
                        tPI = targetType.GetProperty(sPI.Name, allowedFlagsForChildClass);
                    }
                    if (tPI != null && tPI.CanWrite && tPI.PropertyType.IsAssignableFrom(sPI.PropertyType))
                    {
                        tPI.SetValue(target, sPI.GetValue(source, null), null);
                    }
                }
            }
            return target;
        }
    }
}
