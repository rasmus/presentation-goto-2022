using EventFlow.Aggregates;
using EventFlow.Aggregates.ExecutionResults;
using EventFlow.Commands;
using EventFlow.Core;
using EventFlow.EventStores;
using EventFlow.Subscribers;
using Task = System.Threading.Tasks.Task;
// ReSharper disable UnusedMember.Global

namespace Goto2022
{
    public class UserId : Identity<UserId>
    {
        public UserId(string value) : base(value) {}
    }

    [EventVersion("Created", 1)]
    public class CreatedEvent : AggregateEvent<UserAggregate, UserId>
    {
        public string FullName { get; }
        public int Age { get; }

        public CreatedEvent(
            string fullName,
            int age)
        {
            FullName = fullName;
            Age = age;
        }
    }

    [EventVersion("Created", 2)]
    public class CreatedEventV2 : AggregateEvent<UserAggregate, UserId>
    {
        public int Age { get; }

        public CreatedEventV2(
            int age)
        {
            Age = age;
        }
    }

    [EventVersion("NewAge", 1)]
    public class NewAgeEvent : AggregateEvent<UserAggregate, UserId>
    {
        public int Age { get; }

        public NewAgeEvent(
            int age)
        {
            Age = age;
        }
    }

    public class UserAggregate : AggregateRoot<UserAggregate, UserId>,
        IEmit<CreatedEvent>, IEmit<NewAgeEvent>
    {
        // Made public to ease testing
        public string? FullName { get; private set; }
        public int? Age { get; private set; }

        public UserAggregate(UserId id) : base(id) { }

        public IExecutionResult Create(string username, int age)
        {
            if (age < 13)
            {
                return ExecutionResult.Failed("Too young");
            }
            
            Emit(new CreatedEvent(username, age), GetContextMetadata());

            return ExecutionResult.Success();
        }

        public IExecutionResult NewAge(int age)
        {
            if (age < 13)
            {
                return ExecutionResult.Failed("Too young");
            }

            Emit(new NewAgeEvent(age), GetContextMetadata());

            return ExecutionResult.Success();
        }

        public void Apply(CreatedEvent e)
        {
            FullName = e.FullName;
            Age = e.Age;
        }

        public void Apply(NewAgeEvent e)
        {
            Age = e.Age;
        }

        private static Metadata GetContextMetadata()
        {
            return new Metadata(new Dictionary<string, string>
            {
                ["version"] = typeof(UserAggregate).Assembly.GetName().Version?.ToString() ?? "unknown",
            });
        }
    }

    public class CreateUserCommand : Command<UserAggregate, UserId>
    {
        public string FullName { get; }
        public int Age { get; }

        public CreateUserCommand(
            UserId aggregateId,
            string fullName,
            int age)
            : base(aggregateId)
        {
            FullName = fullName;
            Age = age;
        }
    }

    public class CreateUserCommandHandler :
        ICommandHandler<UserAggregate, UserId, IExecutionResult, CreateUserCommand>
    {
        public Task<IExecutionResult> ExecuteCommandAsync(
            UserAggregate aggregate,
            CreateUserCommand createUserCommand,
            CancellationToken cancellationToken)
        {
            var result = aggregate.Create(createUserCommand.FullName, createUserCommand.Age);
            return Task.FromResult(result);
        }
    }

    public class NewUserAgeCommand : Command<UserAggregate, UserId>
    {
        public int Age { get; }

        public NewUserAgeCommand(
            UserId aggregateId,
            int age)
            : base(aggregateId)
        {
            Age = age;
        }
    }

    public class NewUserAgeCommandHandler :
        ICommandHandler<UserAggregate, UserId, IExecutionResult, NewUserAgeCommand>
    {
        public Task<IExecutionResult> ExecuteCommandAsync(
            UserAggregate aggregate,
            NewUserAgeCommand command,
            CancellationToken cancellationToken)
        {
            var result = aggregate.NewAge(command.Age);
            return Task.FromResult(result);
        }
    }

    public class UpdateReadModelWithCreatedUser :
        ISubscribeSynchronousTo<UserAggregate, UserId, CreatedEvent>
    {
        public Task HandleAsync(
            IDomainEvent<UserAggregate, UserId, CreatedEvent> domainEvent,
            CancellationToken cancellationToken)
        {
            Console.WriteLine("Update read model here!");
            return Task.CompletedTask;
        }
    }

    /*
    public class CreatedUserEventV2Upgrader : IEventUpgrader<UserAggregate, UserId>
    {
        public IEnumerable<IDomainEvent<UserAggregate, UserId>> Upgrade(
            IDomainEvent<UserAggregate, UserId> domainEvent)
        {
            throw new NotImplementedException();
        }
    }
    */
}
