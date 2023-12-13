using Microsoft.EntityFrameworkCore;

namespace MassTransitKafkaDemo.Demo;

public class DBctx : DbContext
{
    public DBctx(DbContextOptions<DBctx> options) : base(options)
    {
    }

    public DbSet<OutBoxEvent> OutBox { get; set; }

}
