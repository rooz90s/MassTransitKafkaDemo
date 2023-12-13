using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MassTransitKafkaDemo.Demo;

[Table("outboxevent" , Schema = "dbo")]
public class OutBoxEvent
{
    [Key]
    public int Id { get; set; }
    public string AggregateType { get; set; }
    public string AggregateId { get; set; }
    public string Type { get; set; }
    public string Payload { get; set; }
}
