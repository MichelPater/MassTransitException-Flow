using MassTransit;

namespace MassTransitExceptionFlow;

public class OrderStateState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; }
}

public class OrderStateMachine : MassTransitStateMachine<OrderStateState>
{
    public State Processing { get; private set; }
    public State Completed { get; private set; }
    public Event<OrderSubmitted> OrderSubmitted { get; private set; }

    public OrderStateMachine()
    {
        // Initial state
        InstanceState(x => x.CurrentState);

        // Event handlers
        Event(() => OrderSubmitted, x => x.CorrelateById(m => m.Message.Id));

        Initially(
            When(OrderSubmitted)
                .Then(context =>
                {
                    context.Saga.CorrelationId = context.Message.Id;

                    Console.WriteLine($"Message: '{context.Message.GetType().Name}' has been retried: '{context.GetRetryCount()}' times"); // logging to see retry happening

                    // Enable which exception should be thrown:
                    throw new Exception("some generic message"); // Throws Exception to trigger the retry
                    //throw new ArgumentNullException("some generic message"); // Throws Exception to trigger the retry
                })
                .TransitionTo(Completed));
    }
}