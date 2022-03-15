using EventFlow;
using EventFlow.Aggregates;
using EventFlow.Extensions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

// ReSharper disable StringLiteralTypo

#pragma warning disable CS8618

namespace Goto2022
{
    public class Tests
    {
        private ServiceProvider _serviceProvider;
        private ICommandBus _commandBus;
        private IAggregateStore _aggregateStore;

        [Test]
        public async Task CreateSuccess()
        {
            // Arrange
            var (userId, fullName, age) = (UserId.New, "rasmus", 21);

            // Act
            var executionResult = await _commandBus.PublishAsync(
                new CreateUserCommand(userId, fullName, age),
                CancellationToken.None);

            // Assert
            executionResult.IsSuccess.Should().BeTrue();
            var userAggregate = await _aggregateStore.LoadAsync<UserAggregate, UserId>(
                userId,
                CancellationToken.None);

            userAggregate.FullName.Should().Be(fullName);
            userAggregate.Age.Should().Be(age);
        }

        [Test]
        public async Task CreateFailed()
        {
            // Arrange
            var userId = UserId.New;
            var fullName = "rasmus";
            var age = 11;

            // Act
            var executionResult = await _commandBus.PublishAsync(
                new CreateUserCommand(userId, fullName, age),
                CancellationToken.None);

            // Assert
            executionResult.IsSuccess.Should().BeFalse();
            var userAggregate = await _aggregateStore.LoadAsync<UserAggregate, UserId>(
                userId,
                CancellationToken.None);
            userAggregate.FullName.Should().BeNullOrEmpty();
            userAggregate.Age.Should().BeNull();
        }

        [SetUp]
        public void SetUp()
        {
            _serviceProvider = EventFlowOptions.New()
                .AddDefaults(typeof(Tests).Assembly)
                .ServiceCollection.BuildServiceProvider();
            _commandBus = _serviceProvider.GetRequiredService<ICommandBus>();
            _aggregateStore = _serviceProvider.GetRequiredService<IAggregateStore>();
        }

        [TearDown]
        public async Task TearDown()
        {
            await _serviceProvider.DisposeAsync();
        }
    }
}
