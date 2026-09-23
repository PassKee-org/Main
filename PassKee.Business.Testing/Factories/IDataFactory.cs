using Domain.Abstractions;

namespace PassKee.Business.Testing.Factories;

public interface IDataFactory<TType>: IDomainService where TType : class
{
    TType Generate();
}
