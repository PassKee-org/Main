using System.Collections.Generic;
using System.Threading.Tasks;
using Persistence.Transactions.Behaviors;
using PassKee.Orm.Dao;
using PassKee.Orm.Entities;
using PassKee.Business.Testing.Factories;

namespace PassKee.Business.Testing.Seeders.Entity;

public class UserSeeder : IUserSeeder
{
    private readonly IDataFactory<UserEntity> _userFactory;
    private readonly IUserDao _userDao;
    private readonly IDbSessionProvider _dbSessionProvider;

    public UserSeeder(
        IDbSessionProvider dbSessionProvider,
        IDataFactory<UserEntity> userFactory,
        IUserDao userDao
    )
    {
        _dbSessionProvider = dbSessionProvider;
        _userFactory = userFactory;
        _userDao = userDao;
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
}
