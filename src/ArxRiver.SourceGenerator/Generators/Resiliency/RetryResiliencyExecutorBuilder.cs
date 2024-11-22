using System.CodeDom.Compiler;
using System.Collections.Immutable;
using ArxRiver.SourceGenerator.Extensions;
using ArxRiver.SourceGenerator.Models;

namespace ArxRiver.SourceGenerator.Generators.Resiliency;

internal static class RetryResiliencyExecutor
{
    private static readonly string[] _systemUsings =
    [
        "Polly",
        "Polly.Retry",
        "Polly.Timeout"
    ];


    internal static void Build(IndentedTextWriter writer, TypeSymbolModel type, ImmutableArray<PropertySymbolModel> properties)
    {
        writer.WriteNullableOptions();
        writer.WriteUsings(_systemUsings);
        

        // add class using and namespace...
        if (type.ContainingNamespace is not null)
        {
            writer.WriteLines(
                $$"""

                  namespace {{type.ContainingNamespace}};

                  """);
        }

        writer.WriteLines(
            $$"""
              {{GetAccessModifier(type)}} class RetryResiliencyExecutor
              {
              """);
        writer.Indent++; // method level....

        writer.WriteLines(
            $$"""
              public static async Task ExecuteRetryPipelineAsync(Func<Task> action, RetryStrategyOptions retryStratagyOptions)
              {
                  var retryPipeline = new ResiliencePipelineBuilder()
                      .AddRetry(new RetryStrategyOptions()
                      {
                          Delay = retryStratagyOptions.Delay,
                          BackoffType = retryStratagyOptions.BackoffType,
                          MaxRetryAttempts = retryStratagyOptions.MaxRetryAttempts,
                          UseJitter = retryStratagyOptions.UseJitter,
                          DelayGenerator = retryStratagyOptions.DelayGenerator,
                          MaxDelay = retryStratagyOptions.MaxDelay,
                          Randomizer = retryStratagyOptions.Randomizer,
                          ShouldHandle = retryStratagyOptions.ShouldHandle,
              
                          OnRetry = retryStratagyOptions.OnRetry,
                      })
                      .Build();
              
              
                  await retryPipeline.ExecuteAsync(async _ => await action(), CancellationToken.None);
              }
              
              public static async Task ExecuteRetryPipelineAsync(Func<Task> action, Action<RetryStrategyOptions> configureRetryOptions)
              {
                  var retryOptions = new RetryStrategyOptions();
                  configureRetryOptions(retryOptions);
              
                  await ExecuteRetryPipelineAsync(action, retryOptions);
              }
              
              
              public static async Task<T> ExecuteRetryPipelineAsync<T>(Func<Task<T>> action, RetryStrategyOptions retryStratagyOptions)
              {
                  var retryPipeline = new ResiliencePipelineBuilder()
                      .AddRetry(new RetryStrategyOptions()
                      {
                          Delay = retryStratagyOptions.Delay,
                          BackoffType = retryStratagyOptions.BackoffType,
                          MaxRetryAttempts = retryStratagyOptions.MaxRetryAttempts,
                          UseJitter = retryStratagyOptions.UseJitter,
                          DelayGenerator = retryStratagyOptions.DelayGenerator,
                          MaxDelay = retryStratagyOptions.MaxDelay,
                          Randomizer = retryStratagyOptions.Randomizer,
                          ShouldHandle = retryStratagyOptions.ShouldHandle,
              
                          OnRetry = retryStratagyOptions.OnRetry,
                      })
                      .Build();
              
              
                  return await retryPipeline.ExecuteAsync(async _ => await action(), CancellationToken.None);
              }
              
              
              public static async Task<T> ExecuteRetryPipelineAsync<T>(Func<Task<T>> action, Action<RetryStrategyOptions> configureRetryOptions)
              {
                  var retryOptions = new RetryStrategyOptions();
                  configureRetryOptions(retryOptions);
              
                  return await ExecuteRetryPipelineAsync<T>(action, retryOptions);
              }
              
              public static async Task<T> ExecuteRetryWithTimeoutPipelineAsync<T>(Func<Task<T>> action, int timeoutPeriod,  RetryStrategyOptions retryStratagyOptions)
              {
                  var retryPipeline = new ResiliencePipelineBuilder()
                      .AddRetry(new RetryStrategyOptions()
                      {
                          Delay = retryStratagyOptions.Delay,
                          BackoffType = retryStratagyOptions.BackoffType,
                          MaxRetryAttempts = retryStratagyOptions.MaxRetryAttempts,
                          UseJitter = retryStratagyOptions.UseJitter,
                          DelayGenerator = retryStratagyOptions.DelayGenerator,
                          MaxDelay = retryStratagyOptions.MaxDelay,
                          Randomizer = retryStratagyOptions.Randomizer,
                          ShouldHandle = retryStratagyOptions.ShouldHandle,
              
                          OnRetry = retryStratagyOptions.OnRetry,
                      })
                      .AddTimeout(new TimeoutStrategyOptions
                      {
                          Timeout = TimeSpan.FromSeconds(timeoutPeriod)
                      })
                      .Build();
              
              
                  return await retryPipeline.ExecuteAsync(async _ => await action(), CancellationToken.None);
              }
              
              
              public static async Task<T> ExecuteRetryWithTimeoutPipelineAsync<T>(Func<Task<T>> action, int timeoutPeriod, Action<RetryStrategyOptions> configureRetryOptions)
              {
                  var retryOptions = new RetryStrategyOptions();
                  configureRetryOptions(retryOptions);
              
                  return await ExecuteRetryWithTimeoutPipelineAsync<T>(action, timeoutPeriod, retryOptions);
              }
              
              """);

        writer.Indent--; // end of class
        writer.WriteLine("}");
    }

    private static string GetAccessModifier(TypeSymbolModel classSymbol) =>
        classSymbol.TypeSymbol.DeclaredAccessibility.ToString().ToLowerInvariant();

}