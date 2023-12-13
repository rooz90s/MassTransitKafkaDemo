using System;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Pipeline.Filters;
using MassTransitKafkaDemo.Messages;
using Microsoft.Extensions.Logging;

namespace MassTransitKafkaDemo.Consumers
{
    public class TaskRequestedConsumer : IConsumer<TaskRequested>
    {
        private readonly ILogger<TaskRequestedConsumer> _logger;

        public TaskRequestedConsumer(ILogger<TaskRequestedConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<TaskRequested> context)
        {

            var message = context.Message;
            await Console.Out.WriteLineAsync($"Task {message.Id} requested by {message.RequestedBy} at {message.RequestedDate} - {DateTime.Now}");

        }
    }
}
