using MassTransit;
using MassTransitExceptionFlow;

// Create bus
var builder = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(builder => { })
    .ConfigureServices(services =>
    {
        services.AddLogging(builder => { builder.AddConsole(); });
        services.AddMassTransit(x =>
        {
            x.SetInMemorySagaRepositoryProvider();

            x.AddSagaStateMachine<OrderStateMachine, OrderStateState>(cfg =>
            {
                cfg.UseMessageRetry(r =>
                {
                    r.Immediate(10);
                    
                    // When Exception is thrown
                    r.Handle<Exception>(x => x.Message.Contains("generic")); // Does not work
                    //r.Handle<Exception>(); // Works

                    // When ArgumentNullException is thrown
                    //r.Handle<ArgumentNullException>(x => x.Message.Contains("generic")); // Works 
                    //r.Handle<ArgumentNullException>(); // Works
                });
            });

            x.AddDelayedMessageScheduler(); // There are different schedulers that can be used

            x.UsingRabbitMq((context, config) =>
            {
                config.Host("localhost", "/", host =>
                {
                    host.Username("guest");
                    host.Password("guest");
                });

                config.UseDelayedMessageScheduler();

                config.ConfigureEndpoints(context);
            });
        });
    })
    .Build();

builder.Start();

// Example of sending a request
var publishEndpoint = builder.Services.GetRequiredService<IPublishEndpoint>();

Console.Write("Sending message");
await publishEndpoint.Publish<OrderSubmitted>(new(Guid.NewGuid()));

// Wait for the state machine to complete
await Task.Delay(4000);
Console.WriteLine("Program Execution has completed");

await builder.StopAsync();