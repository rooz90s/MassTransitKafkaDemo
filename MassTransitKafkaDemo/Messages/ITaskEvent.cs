using System;
using Asa.FundBackoffice.Event.Contract;


namespace MassTransitKafkaDemo.Messages
{
    public interface ITaskEvent
    {
        Guid Id { get; }
    }

    // HACK - MassTransit doesn't like object value types so we need a custom
    // base type shared by our event types
    public partial class TaskRequested : ITaskEvent , IIntegrationEvent { }
    public partial class TaskStarted : ITaskEvent , IIntegrationEvent { }
    public partial class TaskCompleted : ITaskEvent , IIntegrationEvent { }
}
