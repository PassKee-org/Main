using System.Collections.Generic;
using System.Threading.Tasks;
using PassKee.Business.Services.Auth;
using PassKee.Business.Testing.Factories;
using PassKee.Orm.Dao;
using PassKee.Orm.Entities;
using Persistence.Transactions.Behaviors;

namespace PassKee.Business.Testing.Seeders.Entity;

public class UserSeeder : IUserSeeder
{
    private readonly IDataFactory<UserEntity> _userFactory;
    private readonly IUserDao _userDao;
    private readonly IDbSessionProvider _dbSessionProvider;
    private readonly IUserAccessTokenDao _accessTokenDao;
    private readonly IJwtAuthService _jwtAuthService;

    public UserSeeder(
        IDbSessionProvider dbSessionProvider,
        IDataFactory<UserEntity> userFactory,
        IUserDao userDao,
        IUserAccessTokenDao accessTokenDao,
        IJwtAuthService jwtAuthService
    )
    {
        _dbSessionProvider = dbSessionProvider;
        _userFactory = userFactory;
        _userDao = userDao;
        _accessTokenDao = accessTokenDao;
        _jwtAuthService = jwtAuthService;
    }

    public async Task<UserEntity> CreateAsync(string? email = null)
    {
        var user = _userFactory.Generate();
        if (!string.IsNullOrEmpty(email))
        {
            user.Email = email;
        }
        await _dbSessionProvider.CurrentSession.SaveAsync(user);
        await _dbSessionProvider.CurrentSession.FlushAsync();
        return user;
    }

    public async Task<ICollection<UserEntity>> CreateAsync(int counter)
    {
        var users = new List<UserEntity>();
        for (int i = 0; i < counter; i++)
        {
            users.Add(await CreateAsync());
        }
        return users;
    }

    public async Task<(string jwtToken, UserEntity user)> CreateAuthorizedAsync(string? email = null)
    {
        var user = await CreateAsync(email);
        var accessToken = await _accessTokenDao.CreateNew(user);
        var jwtToken = _jwtAuthService.BuildJwt(user.Id, accessToken.Id);
        await _dbSessionProvider.CurrentSession.FlushAsync();
        return (jwtToken, user);
    }
}
