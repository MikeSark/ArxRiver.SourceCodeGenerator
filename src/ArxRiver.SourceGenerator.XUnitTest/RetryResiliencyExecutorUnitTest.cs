using ArxRiver.SourceGenerator.XUnitTest;
using JetBrains.Annotations;
using Polly;
using Polly.Retry;
using Polly.Timeout;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;

namespace ArxRiver.SourceGenerator.XUnitTest
{
    [TestSubject(typeof(RetryResiliencyExecutorUnitTest))]
    public class RetryResiliencyExecutorUnitTest
    {
        [Theory]
        [InlineData(3, 1, DelayBackoffType.Constant, false)]
        [InlineData(5, 2, DelayBackoffType.Exponential, true)]
        public async Task ExecuteRetryPipelineAsync_Action_RetryStrategyOptions_ShouldRetryCorrectly(int maxRetryAttempts, int delay, DelayBackoffType DelayBackoffType, bool useJitter)
        {
            var retryOptions = new RetryStrategyOptions
            {
                MaxRetryAttempts = maxRetryAttempts,
                Delay = TimeSpan.FromSeconds(delay),
                BackoffType = DelayBackoffType,
                UseJitter = useJitter,
                ShouldHandle = args => ValueTask.FromResult(args.Outcome.Exception is not null),
                OnRetry = arguments =>
                    {
                        Console.WriteLine($"Retrying... Attempt {arguments.AttemptNumber} afetr delay: {arguments.RetryDelay.TotalSeconds}s, out of {maxRetryAttempts}. Duration: {arguments.Duration.TotalSeconds} ");
                        return new ValueTask(Task.CompletedTask);
                    }
            };

            int attempt = 0;
            Func<Task> action = () =>
                {
                    attempt++;
                    throw new Exception("Test exception");
                };

            await Assert.ThrowsAsync<Exception>(() => RetryResiliencyExecutor.ExecuteRetryPipelineAsync(action, retryOptions));
            attempt.Should().Be(maxRetryAttempts + 1);
        }

        [Theory]
        [InlineData(3, 1, DelayBackoffType.Constant, false)]
        [InlineData(5, 2, DelayBackoffType.Exponential, true)]
        public async Task ExecuteRetryPipelineAsync_Action_ConfigureRetryOptions_ShouldRetryCorrectly(int maxRetryAttempts, int delay, DelayBackoffType DelayBackoffType, bool useJitter)
        {
            Action<RetryStrategyOptions> configureRetryOptions = options =>
                {
                    options.MaxRetryAttempts = maxRetryAttempts;
                    options.Delay = TimeSpan.FromSeconds(delay);
                    options.BackoffType = DelayBackoffType;
                    options.UseJitter = useJitter;
                    options.ShouldHandle = args => ValueTask.FromResult(args.Outcome.Exception is not null);
                    options.OnRetry = arguments =>
                        {
                            Console.WriteLine($"Retrying... Attempt {arguments.AttemptNumber} afetr delay: {arguments.RetryDelay.TotalSeconds}s, out of {options.MaxRetryAttempts}. Duration: {arguments.Duration.TotalSeconds} ");
                            return new ValueTask(Task.CompletedTask);
                        };
                };

            int attempt = 0;
            Func<Task> action = () =>
                {
                    attempt++;
                    throw new Exception("Test exception");
                };

            await Assert.ThrowsAsync<Exception>(() => RetryResiliencyExecutor.ExecuteRetryPipelineAsync(action, configureRetryOptions));
            attempt.Should().Be(maxRetryAttempts + 1);
        }

        [Theory]
        [InlineData(3, 1, DelayBackoffType.Constant, false)]
        [InlineData(5, 2, DelayBackoffType.Exponential, true)]
        public async Task ExecuteRetryPipelineAsync_T_Action_RetryStrategyOptions_ShouldRetryCorrectly(int maxRetryAttempts, int delay, DelayBackoffType DelayBackoffType, bool useJitter)
        {
            var retryOptions = new RetryStrategyOptions
            {
                MaxRetryAttempts = maxRetryAttempts,
                Delay = TimeSpan.FromSeconds(delay),
                BackoffType = DelayBackoffType,
                UseJitter = useJitter,
                ShouldHandle = args => ValueTask.FromResult(args.Outcome.Exception is not null),
                OnRetry = arguments =>
                    {
                        Console.WriteLine($"Retrying... Attempt {arguments.AttemptNumber} afetr delay: {arguments.RetryDelay.TotalSeconds}s, out of {maxRetryAttempts}. Duration: {arguments.Duration.TotalSeconds} ");
                        return new ValueTask(Task.CompletedTask);
                    }
            };

            int attempt = 0;
            Func<Task<int>> action = () =>
                {
                    attempt++;
                    throw new Exception("Test exception");
                };

            await Assert.ThrowsAsync<Exception>(() => RetryResiliencyExecutor.ExecuteRetryPipelineAsync(action, retryOptions));
            attempt.Should().Be(maxRetryAttempts + 1);
        }

        [Theory]
        [InlineData(3, 1, DelayBackoffType.Constant, false)]
        [InlineData(5, 2, DelayBackoffType.Exponential, true)]
        public async Task ExecuteRetryPipelineAsync_T_Action_ConfigureRetryOptions_ShouldRetryCorrectly(int maxRetryAttempts, int delay, DelayBackoffType DelayBackoffType, bool useJitter)
        {
            Action<RetryStrategyOptions> configureRetryOptions = options =>
                {
                    options.MaxRetryAttempts = maxRetryAttempts;
                    options.Delay = TimeSpan.FromSeconds(delay);
                    options.BackoffType = DelayBackoffType;
                    options.UseJitter = useJitter;
                    options.ShouldHandle = args => ValueTask.FromResult(args.Outcome.Exception is not null);
                    options.OnRetry = arguments =>
                        {
                            Console.WriteLine($"Retrying... Attempt {arguments.AttemptNumber} afetr delay: {arguments.RetryDelay.TotalSeconds}s, out of {options.MaxRetryAttempts}. Duration: {arguments.Duration.TotalSeconds} ");
                            return new ValueTask(Task.CompletedTask);
                        };
                };

            int attempt = 0;
            Func<Task<int>> action = () =>
                {
                    attempt++;
                    throw new Exception("Test exception");
                };

            await Assert.ThrowsAsync<Exception>(() => RetryResiliencyExecutor.ExecuteRetryPipelineAsync(action, configureRetryOptions));
            attempt.Should().Be(maxRetryAttempts + 1);
        }

        [Theory]
        [InlineData(3, 1, 5, DelayBackoffType.Constant, false)]
        [InlineData(5, 2, 10, DelayBackoffType.Exponential, true)]
        public async Task ExecuteRetryWithTimeoutPipelineAsync_T_Action_TimeoutPeriod_RetryStrategyOptions_ShouldRetryAndTimeoutCorrectly(int maxRetryAttempts, int delay, int timeoutPeriod, DelayBackoffType DelayBackoffType, bool useJitter)
        {
            var retryOptions = new RetryStrategyOptions
            {
                MaxRetryAttempts = maxRetryAttempts,
                Delay = TimeSpan.FromSeconds(delay),
                BackoffType = DelayBackoffType,
                UseJitter = useJitter,
                ShouldHandle = args => ValueTask.FromResult(args.Outcome.Exception is not null),
                OnRetry = arguments =>
                    {
                        Console.WriteLine($"Retrying... Attempt {arguments.AttemptNumber} afetr delay: {arguments.RetryDelay.TotalSeconds}s, out of {maxRetryAttempts}. Duration: {arguments.Duration.TotalSeconds} ");
                        return new ValueTask(Task.CompletedTask);
                    }
            };

            int attempt = 0;
            Func<Task<int>> action = () =>
                {
                    attempt++;
                    throw new Exception("Test exception");
                };

            await Assert.ThrowsAsync<Exception>(() => RetryResiliencyExecutor.ExecuteRetryWithTimeoutPipelineAsync(action, timeoutPeriod, retryOptions));
            attempt.Should().Be(maxRetryAttempts + 1);
        }

        [Theory]
        [InlineData(3, 1, 5, DelayBackoffType.Constant, false)]
        [InlineData(5, 2, 10, DelayBackoffType.Exponential, true)]
        public async Task ExecuteRetryWithTimeoutPipelineAsync_T_Action_TimeoutPeriod_ConfigureRetryOptions_ShouldRetryAndTimeoutCorrectly(int maxRetryAttempts, int delay, int timeoutPeriod, DelayBackoffType DelayBackoffType, bool useJitter)
        {
            Action<RetryStrategyOptions> configureRetryOptions = options =>
                {
                    options.MaxRetryAttempts = maxRetryAttempts;
                    options.Delay = TimeSpan.FromSeconds(delay);
                    options.BackoffType = DelayBackoffType;
                    options.UseJitter = useJitter;
                    options.ShouldHandle = args => ValueTask.FromResult(args.Outcome.Exception is not null);
                    options.OnRetry = arguments =>
                        {
                            Console.WriteLine($"Retrying... Attempt {arguments.AttemptNumber} afetr delay: {arguments.RetryDelay.TotalSeconds}s, out of {options.MaxRetryAttempts}. Duration: {arguments.Duration.TotalSeconds} ");
                            return new ValueTask(Task.CompletedTask);
                        };
                };

            int attempt = 0;
            Func<Task<int>> action = () =>
                {
                    attempt++;
                    throw new Exception("Test exception");
                };

            await Assert.ThrowsAsync<Exception>(() => RetryResiliencyExecutor.ExecuteRetryWithTimeoutPipelineAsync(action, timeoutPeriod, configureRetryOptions));
            attempt.Should().Be(maxRetryAttempts + 1);
        }
    }
}