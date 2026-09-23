using Domain.Abstractions;

namespace PassKee.Orm.Core;

public class AEntity: IEntity
{
    public virtual Guid Id { get; set; }

    private int? _hashCode;
    public override int GetHashCode()
    {
        if (_hashCode.HasValue)
            return _hashCode.Value;

        var ísTransientEntity = Id == Guid.Empty;
        if (ísTransientEntity)
        {
            _hashCode = base.GetHashCode();
            return _hashCode.Value;
        }

        return Id.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        if (obj is IHasId other)
        {
            var thisIsTransient = Id == Guid.Empty;
            var otherIsTransient = Id == Guid.Empty;
        
            if (thisIsTransient && otherIsTransient)
                return ReferenceEquals(this, other);

            return Id == other.Id;    
        }
        return ReferenceEquals(this, obj);
    }

    public static bool operator ==(AEntity? a, AEntity? b)
    {
        return Equals(a, b);
    }

    public static bool operator !=(AEntity? a, AEntity? b)
    {
        return !Equals(a, b);
    }
    
    public override string ToString()
    {
        return Id.ToString();
    }

    #region Audit/Status

    public virtual DateTime? DeletedAt { get; set; }
    public virtual DateTime? UpdatedAt { get; set; }
    public virtual DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public virtual bool IsDeleted => DeletedAt != null;
    
    public virtual bool IsNew => Id == Guid.Empty;
    
    #endregion
}
