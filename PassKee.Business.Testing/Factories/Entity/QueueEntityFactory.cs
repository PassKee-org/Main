using System;
using Bogus;
using PassKee.Orm.Constants;
using PassKee.Orm.Entities;

namespace PassKee.Business.Testing.Factories.Entity;

public class QueueEntityFactory : IDataFactory<QueueEntity>
{
    private readonly Faker<QueueEntity> _factory;

    public QueueEntityFactory()
    {
        _factory = new Faker<QueueEntity>()
            .RuleFor(fake => fake.Status, fake => QueueStatus.Pending)
            .RuleFor(fake => fake.Channel, fake => QueueChannel.Default)
            .RuleFor(fake => fake.Priority, fake => QueuePriority.Normal)
            .RuleFor(fake => fake.ContextType, fake => fake.Random.Word())
            .RuleFor(fake => fake.ContextData, fake => "{}")
            .RuleFor(fake => fake.ProcessAt, fake => DateTime.UtcNow)
            .RuleFor(fake => fake.CreatedAt, fake => DateTime.UtcNow)
            .RuleFor(fake => fake.UpdatedAt, fake => DateTime.UtcNow);
    }

    public QueueEntity Generate()
    {
        return _factory.Generate();
    }
}

