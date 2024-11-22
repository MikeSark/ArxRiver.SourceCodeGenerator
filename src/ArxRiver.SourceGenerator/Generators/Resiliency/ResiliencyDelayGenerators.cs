using System.CodeDom.Compiler;
using System.Collections.Immutable;
using ArxRiver.SourceGenerator.Extensions;
using ArxRiver.SourceGenerator.Models;

namespace ArxRiver.SourceGenerator.Generators.Resiliency;

internal static class ResiliencyDelayGenerators
{
    
    internal static void Build(IndentedTextWriter writer, TypeSymbolModel type, ImmutableArray<PropertySymbolModel> properties)
    {
        writer.WriteNullableOptions();

        // add class using and namespace...
        if (type.ContainingNamespace is not null)
        {
            writer.WriteLines(
                $$"""

                  namespace {{type.ContainingNamespace.Replace(type.Name, "").Trim('.')}};

                  """);
        }

        writer.WriteLines(
            $$"""
              {{GetAccessModifier(type)}} sealed class ResiliencyDelayGenerators
              {
              """);
        writer.Indent++; // method level....

        writer.WriteLines(
            $$"""
              /// <summary>
              /// Generates a fixed delay generator
              /// </summary>
              /// <param name="delayInSeconds"></param>
              /// <code>
              /// var fixedDelayGenerator = FixedDelay(TimeSpan.FromSeconds(3));
              /// </code>
              /// <returns></returns>
              public static Func<int, TimeSpan> FixedDelayGenerator(int delayInSeconds) =>
                  attempt => TimeSpan.FromSeconds(delayInSeconds);
              
              
              /// <summary>
              /// Generates a linear backoff delay generator
              /// </summary>
              /// <param name="initialDelay"></param>
              /// <param name="increment"></param>
              /// <code>
              /// var linearBackoffGenerator = LinearBackoff(TimeSpan.FromSeconds(1), TimeSpan.FromMilliseconds(500));
              /// </code>
              /// <returns></returns>
              public static Func<int, TimeSpan> LinearBackoffDelayGenerator(int initialDelay, int increment) =>
                  attempt => TimeSpan.FromSeconds(initialDelay) + (TimeSpan.FromSeconds(increment) * (attempt - 1));
              
              
              /// <summary>
              /// Generates an exponential backoff delay generator with Jitter
              /// </summary>
              /// <param name="initialDelayInDescond"></param>
              /// <param name="maxExponent"></param>
              /// <code>
              /// var exponentialJitterGenerator = ExponentialBackoffWithJitter(TimeSpan.FromSeconds(1), 5);
              /// </code>
              /// <returns></returns>
              public static Func<int, TimeSpan> ExponentialBackoffWithJitter(int initialDelayInDescond, int maxExponent)
              {
                  var random = new Random();
                  return attempt =>
                      {
                          var exponent = Math.Min(attempt, maxExponent);
                          var delay = (initialDelayInDescond * 1000) * Math.Pow(2, exponent);
                          var jitter = random.NextDouble() * delay * 0.3; // Adds 0-30% jitter
                          return TimeSpan.FromMilliseconds(delay + jitter);
                      };
              }
              
              
              /// <summary>
              /// 
              /// </summary>
              /// <param name="initialDelay"></param>
              /// <param name="maxDelay"></param>
              /// <code>
              /// var increasingRandomDelayGenerator = IncreasingRandomDelayWithMaxCap(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(10));
              /// </code>
              /// <returns></returns>
              public static Func<int, TimeSpan> IncreasingRandomDelayWithMaxCap(int initialDelay, int maxDelay)
              {
                  var random = new Random();
                  return attempt =>
                      {
                          var initialDelayTimespan = TimeSpan.FromSeconds(initialDelay);
                          var maxDelayTimespan = TimeSpan.FromSeconds(maxDelay);
                          
                          var delay = initialDelayTimespan.TotalMilliseconds * attempt;
                          var jitter = random.NextDouble() * delay * 0.2; // Adds 0-20% jitter
                          delay = Math.Min(delay + jitter, maxDelayTimespan.TotalMilliseconds);
                          return TimeSpan.FromMilliseconds(delay);
                      };
              }
              
              
              /// <summary>
              /// Generates a random delay generator
              /// </summary>
              /// <param name="minDelay"></param>
              /// <param name="maxDelay"></param>
              /// <code>
              /// var randomDelayGenerator = RandomDelayInRange(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5));
              /// </code>
              /// <returns></returns>
              public static Func<int, TimeSpan> RandomDelayInRange(int minDelay, int maxDelay)
              {
                  var random = new Random();
                  return attempt =>
                      {
                          var minDelayTimespan = TimeSpan.FromSeconds(minDelay);
                          var maxDelayTimespan = TimeSpan.FromSeconds(maxDelay);
              
                          var delay = minDelayTimespan.TotalMilliseconds + (random.NextDouble() * (maxDelayTimespan - minDelayTimespan).TotalMilliseconds);
                          return TimeSpan.FromMilliseconds(delay);
                      };
              }
              
              
              /// <summary>
              /// Generates an exponential backoff delay generator
              /// </summary>
              /// <param name="initialDelay"></param>
              /// <code>
              /// var fibonacciBackoffGenerator = FibonacciBackoff(TimeSpan.FromMilliseconds(500));
              /// </code>
              /// <returns></returns>
              public static Func<int, TimeSpan> FibonacciBackoff(TimeSpan initialDelay)
              {
                  return attempt =>
                      {
                          var fib = Fibonacci(attempt);
                          return TimeSpan.FromMilliseconds(initialDelay.TotalMilliseconds * fib);
                      };
              }
              
              
              /// <summary>
              /// Calculates the nth Fibonacci number.
              /// </summary>
              /// <param name="n">The position in the Fibonacci sequence to calculate.</param>
              /// <returns>The nth Fibonacci number.</returns>
              private static int Fibonacci(int n)
              {
                  if (n <= 1) return n;
                  int previous = 0, current = 1;
              
                  for (var i = 2; i <= n; i++)
                  {
                      var next = previous + current;
                      previous = current;
                      current = next;
                  }
              
                  return current;
              }
              
              """);

        writer.Indent--; // end of class
        writer.WriteLine("}");
    }

    private static string GetAccessModifier(TypeSymbolModel classSymbol) =>
        classSymbol.TypeSymbol.DeclaredAccessibility.ToString().ToLowerInvariant();

}