using System.Diagnostics.CodeAnalysis;
using Domain.Abstractions;
using PassKee.Business.Common.Resources;

namespace PassKee.Business.Common.Exceptions.Common
{
    public class DataValidationException : Exception, IDomainException
    {
        public DataValidationException(): this(RG.Error_DataValidationException)
        {
        }

        public DataValidationException(string message) : base(message)
        {
        }
        
        public static void ThrowIfNull(params object?[] arguments)
        {
            foreach (var argument in arguments)
            {
                ThrowIfNull(argument);
            }
        }
    
        public static void ThrowIfNull([NotNull] object? argument, string? message = null)
        {
            if (argument is null)
            {
                Throw(message);
            }
        }
    
        [DoesNotReturn]
        internal static void Throw(string? message) =>
            throw new DataValidationException(message);
    }
}
