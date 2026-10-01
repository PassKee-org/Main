using Bogus;
using PassKee.Orm.Entities;

namespace PassKee.Business.Testing.Factories.Entity;

public class UserEntityFactory : IDataFactory<UserEntity>
{
    private readonly Faker<UserEntity> _factory;

    public UserEntityFactory()
    {
        _factory = new Faker<UserEntity>()
            .RuleFor(fake => fake.Email, fake => fake.Person.Email)
            .RuleFor(fake => fake.CreatedAt, fake => fake.Date.Past().ToUniversalTime())
            .RuleFor(fake => fake.UpdatedAt, fake => fake.Date.Past().ToUniversalTime());
    }

    public UserEntity Generate()
    {
        return _factory.Generate();
    }
}
