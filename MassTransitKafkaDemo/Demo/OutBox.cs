using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MassTransitKafkaDemo.Demo;

[Table("outbox" , Schema = "dbo")]
public class OutBoxEvent
{
    [Key]
    public Guid Id { get; set; }
    public string Context { get; set; }
    public string AggregateId { get; set; }
    public byte[] Payload { get; set; }
}
