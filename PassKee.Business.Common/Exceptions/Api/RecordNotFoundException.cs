using System.Diagnostics.CodeAnalysis;
using Domain.Abstractions;

namespace PassKee.Business.Common.Exceptions.Api
{
    public class RecordNotFoundException : Exception, IDomainException
    {
        public RecordNotFoundException(string? message = "Record was not found") : base(message ?? "Record was not found")
        {
        }
        
        public static void ThrowIfNull([NotNull] object? argument, string? message = null)
        {
            if (argument is null)
            {
                Throw(message);
            }
        }
    
        [DoesNotReturn]
        internal static void Throw(string? message) => throw new RecordNotFoundException(message ?? "Record was not found");
    }
}
