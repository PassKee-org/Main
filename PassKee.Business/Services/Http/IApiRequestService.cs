using System.Threading.Tasks;
using Domain.Abstractions.Api;
using PassKee.Orm.Entities;

namespace PassKee.Business.Services.Http;

public interface IApiRequestService : IBaseApiRequestService
{
    Task<UserEntity> GetCurrentUser();
    Task<UserEntity?> GetCurrentUserOrNull();
}
