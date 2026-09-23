using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Abstractions;
using PassKee.Orm.Entities;

namespace PassKee.Business.Testing.Seeders.Entity;

public interface IUserSeeder : IDomainService
{
    Task<UserEntity> CreateAsync(string? email = null);
    Task<ICollection<UserEntity>> CreateAsync(int counter);
}
