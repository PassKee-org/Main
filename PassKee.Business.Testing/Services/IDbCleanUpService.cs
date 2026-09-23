using Domain.Abstractions;

namespace PassKee.Business.Testing.Services;

public interface IDbCleanUpService: IDomainService
{
    Task CleanUp();
}
