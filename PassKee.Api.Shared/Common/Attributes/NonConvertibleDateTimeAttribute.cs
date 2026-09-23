namespace PassKee.Api.Shared.Common.Attributes;

/// <summary>
/// Used for: UserTimeZoneDateConverter
/// 
/// Mark a DateTime / DateTime? property with this attribute to skip timezone conversion.
/// Wire up <see cref="ContractResolver"/> in <c>JsonSerializerSettings</c> for the
/// attribute to be honoured.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class NonConvertibleDateTimeAttribute : Attribute { }
