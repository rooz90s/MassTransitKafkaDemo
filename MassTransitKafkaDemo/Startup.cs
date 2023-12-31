using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Asa.FundBackoffice.Event.AvroRegistry;
using Asa.FundBackoffice.Event.Contract;
using Confluent.Kafka;
using Confluent.Kafka.SyncOverAsync;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using GreenPipes;
using GreenPipes.Filters;
using GreenPipes.Pipes;
using GreenPipes.Specifications;
using MassTransit;
using MassTransit.ConsumeConfigurators;
using MassTransit.ConsumerSpecifications;
using MassTransit.Context.Converters;
using MassTransit.KafkaIntegration;
using MassTransit.PipeConfigurators;
using MassTransit.Pipeline.Filters;
using MassTransit.Pipeline.Pipes;
using MassTransit.Transports;
using MassTransitKafkaDemo.Consumers;
using MassTransitKafkaDemo.Demo;
using MassTransitKafkaDemo.exceptionHandlers;
using MassTransitKafkaDemo.Infrastructure.AvroSerializers;
using MassTransitKafkaDemo.Messages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace MassTransitKafkaDemo
{
    public class Startup
    {
        private const string TaskEventsTopic = "issuing.events";
        private const string KafkaBroker = "broker:29092";
        private const string SchemaRegistryUrl = "schema-registry:8081";


        public void ConfigureServices(IServiceCollection services)
        {
            var schemaRegistryConfig = new SchemaRegistryConfig { Url = SchemaRegistryUrl };
            var schemaRegistryClient = new CachedSchemaRegistryClient(schemaRegistryConfig);
            services.AddSingleton<ISchemaRegistryClient>(schemaRegistryClient);

            services.AddDbContext<DBctx>(conf =>
            {
                conf.UseSqlServer("Data Source =172.22.3.58,14333; Initial Catalog = OutboxCDC ; user Id= sa; pwd=Password!; TrustServerCertificate=True");
            });

            services.AddAsaSchemaRegistry(builder =>
            {
                builder.SetSchemaRegistry(config => config.Url = SchemaRegistryUrl)
                    .AddProducerType(typeBuilder => typeBuilder
                        .AddType<TaskRequested>()
                        .AddType<TaskStarted>()
                        .AddType<TaskCompleted>()
                        .Build()
                        .SetAvroSerializer(config =>
                        {
                            config.SubjectNameStrategy = SubjectNameStrategy.Record;
                            config.AutoRegisterSchemas = true;
                        })
                    )
                    .AddConsumeType(typeBuilder =>
                        typeBuilder.AddType<TaskRequested>()
                            .AddType<TaskStarted>()
                            .AddType<TaskCompleted>()
                            .Build()
                    );
            });

            services.AddMassTransit(busConfig =>
            {
                busConfig.UsingInMemory((context, config) =>
                {
                    config.UseExceptionLogger();
                    config.ConfigureEndpoints(context);
                });
                busConfig.AddRider(riderConfig =>
                {

                    // Set up producers - events are produced by DemoProducer hosted service
                    // riderConfig.AddProducer<string, ITaskEvent>(TaskEventsTopic, (riderContext, producerConfig) =>
                    // {
                    //     // Serializer configuration.
                    //
                    //     // Important: Use either SubjectNameStrategy.Record or SubjectNameStrategy.TopicRecord.
                    //     // SubjectNameStrategy.Topic (default) would result in the topic schema being set based on
                    //     // the first message produced.
                    //     //
                    //     // Note that you can restrict the range of message types for a topic by setting up the
                    //     // topic schema using schema references. This hasn't yet been covered in this demo - more
                    //     // details available here:
                    //     // https://docs.confluent.io/platform/current/schema-registry/serdes-develop/index.html#multiple-event-types-in-the-same-topic
                    //     // https://docs.confluent.io/platform/current/schema-registry/serdes-develop/serdes-avro.html#multiple-event-types-same-topic-avro
                    //     // https://www.confluent.io/blog/multiple-event-types-in-the-same-kafka-topic/
                    //     var serializerConfig = new AvroSerializerConfig { SubjectNameStrategy = SubjectNameStrategy.Record, AutoRegisterSchemas = true, };
                    //
                    // var serializer = new MultipleTypeSerializer<ITaskEvent>(multipleTypeConfig, schemaRegistryClient, serializerConfig);
                    //     // Note that all child serializers share the same AvroSerializerConfig - separate producers could
                    //     // be used for each logical set of message types (e.g. all messages produced to a certain topic)
                    //     // to support varying configuration if needed.
                    //  producerConfig.SetKeySerializer(new AvroSerializer<string>(schemaRegistryClient).AsSyncOverAsync());
                    //     producerConfig.SetValueSerializer(serializer.AsSyncOverAsync());
                    // });

                    // Set up consumers and consuming
                    riderConfig.AddConsumersFromNamespaceContaining<TaskRequestedConsumer>();

                    riderConfig.UsingKafka((riderContext, kafkaConfig) =>
                    {
                        var deserializer = riderContext.GetService<IDeserializer>();
                        kafkaConfig.Host(KafkaBroker);

                        var groupId = "g1"; //Guid.NewGuid().ToString(); // always start from beginning
                        kafkaConfig.TopicEndpoint<string, IIntegrationEvent>(TaskEventsTopic, groupId, tc =>
                        {
                            // tc.UseKillSwitch(options =>
                            // {
                            //     options.SetActivationThreshold(3)
                            //         .SetTripThreshold(0.15)
                            //         .SetRestartTimeout(ms: 8000);
                            // });

                            tc.ConfigureReceive(configurator => configurator.UseExceptionLogger());

                            tc.AutoOffsetReset = AutoOffsetReset.Earliest;
                            tc.SetKeyDeserializer(new AvroDeserializer<string>(schemaRegistryClient, null).AsSyncOverAsync());

                            tc.SetValueDeserializer(deserializer.AsSyncOverAsync());

                            //tc.UseRawJsonSerializer();

                            tc.ConsumerMessageConfigured(new ConsumerMessageSpecification<TaskStartedConsumer, TaskStarted>());
                            tc.ConsumerMessageConfigured(new ConsumerMessageSpecification<TaskRequestedConsumer, TaskRequested>());
                            tc.ConsumerMessageConfigured(new ConsumerMessageSpecification<TaskRequestedConsumer, TaskRequested>());

                            tc.ConfigureConsumer<TaskRequestedConsumer>(riderContext);
                            tc.ConfigureConsumer<TaskStartedConsumer>(riderContext);
                        });
                    });
                });
            });
            services.AddMassTransitHostedService();

            // This fella produces the events
            services.AddHostedService<DemoProducer>();
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Hello World!");
                });
            });
        }
    }
}
