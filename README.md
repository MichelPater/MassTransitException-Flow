# MassTransit Exception Flow Example
A very simple solution that aims to understand the exception handling in MassTransit.

This example sets up a Statemachine and publishes a `OrderSubmitted` event. 


The Statemachine can either throw an generic `Exception` or a `ArgumentNullException` (see `OrderStateMachine.cs` line 34 & 35). MessageRetry is configured in `Program.cs`, other cases have been added and work in tandem with the exception that is thrown in the `OrderStateMachine.cs`.

# Problem
The behaviour for triggering the retry mechanism seems to be different when handling the generic `Exception` with a filter. Meaning when adding filtering on Message content (`r.Handle<Exception>(x => x.Message.Contains("generic"));`) retries will no longer be done. This does not happen when trying other exceptions and filtering on content.


```csharp

cfg.UseMessageRetry(r =>
{
	r.Immediate(10);
	
	// When Exception is thrown
	r.Handle<Exception>(x => x.Message.Contains("generic")); // Does not work <============= This handle seems to result in not triggering the retry even though all other examples do
	//r.Handle<Exception>(); // Works

	// When ArgumentNullException is thrown
	//r.Handle<ArgumentNullException>(x => x.Message.Contains("generic")); // Works 
	//r.Handle<ArgumentNullException>(); // Works
});

```

These flows work as expected:
- When throwing a generic `Exception` and adding `r.Handle<Exception>();` (without any other filters) retry is triggered.
- When throwing a `ArgumentNullException` and adding `r.Handle<ArgumentNullException>(x => x.Message.Contains("generic"));` (without any other filters) retry is triggered.
- When throwing a `ArgumentNullException` and adding `r.Handle<ArgumentNullException>();` (without any other filters) retry is triggered.

