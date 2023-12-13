using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GreenPipes;
using MassTransit;

namespace MassTransitKafkaDemo.exceptionHandlers;

public static class ExampleMiddlewareConfiguratorExtensions
{
    public static void UseExceptionLogger<T>(this IPipeConfigurator<T> configurator)
        where T : class, PipeContext
    {
        configurator.AddPipeSpecification(new ExceptionLoggerSpecification<T>());
    }
    // public static void UseExceptionLogger1<T>(this IPipeSpecification<ConsumeContext> specification)
    //     where T : class, PipeContext
    // {
    //
    //     specification.Apply();
    // }
}

public class ExceptionLoggerSpecification<T> :
    IPipeSpecification<T>
    where T : class, PipeContext
{
    public IEnumerable<ValidationResult> Validate()
    {
        return Enumerable.Empty<ValidationResult>();
    }

    public void Apply(IPipeBuilder<T> builder)
    {
        builder.AddFilter(new ExceptionLoggerFilter<T>());
    }
}

public class ExceptionLoggerFilter<T> :
    IFilter<T>
    where T : class, PipeContext
{
    public void Probe(ProbeContext context)
    {
        context.CreateFilterScope("exceptionLogger");
    }

    /// <summary>
    /// Send is called for each context that is sent through the pipeline
    /// </summary>
    /// <param name="context">The context sent through the pipeline</param>
    /// <param name="next">The next filter in the pipe, must be called or the pipe ends here</param>
    public async Task Send(T context, IPipe<T> next)
    {
        try
        {
            await next.Send(context);
        }
        catch (MessageNotConsumedException _)
        {
            //ignore
        }
        catch (Exception ex)
        {
            await Console.Out.WriteLineAsync($"An exception occurred: {ex.Message}");

            throw;
        }
    }
}
