using System.Diagnostics;
using Api.Requests.Abstractions;
using Domain.Abstractions;

namespace PassKee.Api.Shared.Dto.Entity.Common;

public abstract class BaseDto: IResponse, IHasId
{
    public Guid Id { get; set; }
    
    public override bool Equals(object? obj)
    {
        if (obj is IHasId other)
        {
            var thisIsTransient = Id == Guid.Empty;
            var otherIsTransient = other.Id == Guid.Empty;
        
            if (thisIsTransient && otherIsTransient)
                return ReferenceEquals(this, other);

            return Id == other.Id;    
        }
        return ReferenceEquals(this, obj);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public static bool operator ==(BaseDto? a, BaseDto? b)
    {
        return Equals(a, b);
    }

    public static bool operator !=(BaseDto? a, BaseDto? b)
    {
        return !Equals(a, b);
    }
    
    public override string ToString()
    {
        return Id.ToString();
    }
}
