# Routemeister
Routemeister is a small lib built with one single purpose. **Effectively performing in-process async message routing.** It can be used if you e.g. are dispatching messages cross process using RabbitMQ or ActiveMQ and then want to dispatch the message to typed handlers within the consuming process.

It supports Routers and/or Dispatchers; the difference being the router just routes the message to the endpoint, while the dispatcher supports different messaging strategies like: `Send`, `Publish` and `Request-Response`.

It's built against .NET Standard v1.4

## External writings
- [How to create a simple in memory aggregator using Routemeister](https://danielwertheim.se/how-to-create-a-simple-in-memory-aggregator-using-routemeister/)
- [Routemeister how to support request response](http://danielwertheim.se/routemeister-how-to-support-request-response/)
- [Routemeister reaches v1](http://danielwertheim.se/routemeister-reaches-v1/)
- [Routemeister and middlewares](http://danielwertheim.se/routemeister-and-middlewares/)
- [Routemeister and Autofac](http://danielwertheim.se/routemeister-and-autofac/)
- [Reply to please compare Routemesiter with MediatR](http://danielwertheim.se/reply-to-please-compare-routemesiter-with-mediatr/)
- [Routemeister optimizations](http://danielwertheim.se/routemeister-optimizations/)
- [Introducing Routemeister](http://danielwertheim.se/introducing-routemeister/)

## Release notes
Release notes are [kept here](ReleaseNotes.md).

## Install
First, install it. It's distributed [via NuGet](https://www.nuget.org/packages/Routemeister).

```
install-package routemeister
```

## Message handler definition
A message handler definition is a simple `interface` that defines the message handler methods.

If you want, you can create this interface in your own project, e.g. if you don't like the name or something. But otherwise, you can use any of the builtin ones:

- `IAsyncMessageHandler` (for use with routers and/or dispatchers)
- `IAsyncRequestHandler` (for use with dispatchers)

### Sample of custom definition
This is **NOT required**. This is optional and only something you would do if you want to own the interface for some reason. The requirements are:

- Generic interface, with one generic argument (name it what ever you want)
- Should contain a single method (name it what ever you want)
- The method should accept one argument only (the message)
- The method should return `Task`.

```csharp
//You own this interface. Create it and name it as you want.
public interface IHandle<in T>
{
    //Can be named whatever.
    //Must return a Task
    //Must take one class argument only
    Task HandleAsync(T message);
}
```

## Concrete route handler
Either use the pre-made `IAsyncMessageHandler` or define your own interface and create some concrete handlers (classes implementing any of these interfaces). Each class can naturally implement the interface multiple times to handle different types of messages. Each message type being processed can (if you want) be processed by multiple message handlers (different classes).

```csharp
public class MyHandler :
    IAsyncMessageHandler<MyConcreteMessage>,
    IAsyncMessageHandler<ISomeInterfaceMessage>
{
    public Task HandleAsync(MyConcreteMessage message)
    {
        //Do something with the message
    }

    public Task HandleAsync(ISomeInterfaceMessage message)
    {
        //Do something with the message
    }
}

public class SomeOtherHandler :
    IAsyncMessageHandler<MyConcreteMessage>,
    IAsyncMessageHandler<ISomeInterfaceMessage>
{
    public Task HandleAsync(MyConcreteMessage message)
    {
        //Do something with the message
    }

    public Task HandleAsync(ISomeInterfaceMessage message)
    {
        //Do something with the message
    }
}
```

## Create routes
In order to invoke the message handlers defined above, you will use an implementation of `IAsyncRouter`. These needs `MessageRoutes` which contains many `MessageRoute` instances.

The created `MessageRoutes` should be kept around. **Don't recreate them all the time**.

You create message routes using a `MessageRouteFactory`. The factory needs to know in what assemblies to look for message handlers. It also needs to know which interface is used as the marker interface.

```csharp
var factory = new MessageRouteFactory();
var routes = factory.Create(
    typeof(SomeType).Assembly,
    typeof(IAsyncMessageHandler<>));
```

You can of course add from many different assemblies or marker interfaces:

```csharp
var factory = new MessageRouteFactory();
var routes = factory.Create(
    new [] { assembly1, assembly2 },
    typeof(IAsyncMessageHandler<>));

routes.Add(factory.Create(
    new [] { assembly1, assembly2 },
    typeof(IAsyncMessageHandler<>)));

routes.Add(factory.Create(
    new [] { assembly1, assembly2, assembly3 },
    typeof(IAnotherHandle<>)));
```

### More info about MessageRoute
If you are interested... Each message route has a `MessageType` and one-to-many `Actions`. Each `Action` represents a message handler.

## Use the routes
The `routes` can now be used as you want to route messages manually, or you can start using an existing router:

- `SequentialAsyncMessageRouter.RouteAsync(...)`
- `MiddlewareEnabledAsyncMessageRouter.RouteAsync(...)`
- `AsyncDispatcher.SendAsync(...) | PublishAsync(...) | RequestAsync<TResponse>(...)`

Neither of these routers are complex. They route sequentially and fail immediately if an exception is thrown. The simplest one is `SequentialAsyncRouter`. Go and have a look. **You can easily create your own**.

```csharp
var router = new SequentialAsyncRouter(
    messageHandlerCreator, //See more below
    routes);

await router.RouteAsync(new MyConcreteMessage
{
    //Some data
}).ConfigureAwait(false);
```

## Strategy for resolving the message handlers
To the existing `routers`, you pass a `MessageHandlerCreator` delegate, which is responsible for creating an instance of the message handler class.

```csharp
public delegate object MessageHandlerCreator(Type messageHandlerType, MessageEnvelope envelope);
```

This is where you decide on how the handlers should be created and where you would **hook in your IoC-container**. You could pass `Activator.CreateInstance(handlerType)` but you probably want to use your IoC container or something, so that additional dependencies are resolved via the container.

```csharp
//Using Activator
var router = new SequentialAsyncRouter(
    (handlerType, envelope) => Activator.CreateInstance(handlerType),
    routes);

//Using IoC
var router = new SequentialAsyncRouter(
    (handlerType, envelope) => yourContainer.Resolve(handlerType),
    routes);
```

The `MessageEnvelope` is something you could make use of to carry state e.g. using the `MiddlewareEnabledAsyncRouter`. This lets you register hooks into the message pipeline via `router.Use`. Hence you could use that to e.g. acomplish per-request scope with your IoC. See below for sample using Autofac.

Sample using [Autofac Lifetime scopes](http://docs.autofac.org/en/latest/lifetime/working-with-scopes.html) to get per request resolving

**Sample using** `SequentialAsyncRouter`
```csharp
var router = new SequentialAsyncRouter(
    (handlerType, envelope) => envelope.GetScope().Resolve(handlerType),
    routes)
{
    OnBeforeRouting = envelope => envelope.SetScope(parentscope.BeginLifetimeScope()),
    OnAfterRouted = envelope => envelope.GetScope()?.Dispose()
};
```

**Sample using** `MiddlewareEnabledAsyncRouter`
```csharp
var router = new MiddlewareEnabledAsyncRouter(
    (handlerType, envelope) => envelope.GetScope().Resolve(handlerType),
    routes);

router.Use(next => async envelope =>
{
    using(var scope = parentscope.BeginLifetimeScope())
    {
        envelope.SetScope(scope);
        return next(envelope);
    }
});
```

In this case `SetScope` and `GetScope` are just custom **extenion methods** that accesses `envelope.SetState("scope", scope)` and `envelope.GetState("scope") as Autofac.ILifetimeScope`.

Now, when ever a message is routed, it will be passed through the pipeline that you hooked into above.

```csharp
await router.RouteAsync(new MyConcreteMessage
{
    //Some data
}).ConfigureAwait(false);
```

## Manual routing
You could of course use the produced `MessageRoutes` manually.

```csharp
var message = new MyConcreteMessage
{
    //Some data
};
var messageType = message.GetType();
var route = routes.GetRoute(messageType);
foreach (var action in route.Actions)
    await action.Invoke(GetHandler(action.HandlerType), message).ConfigureAwait(false);
```

## Performance/Numbers
Below are some numbers comparing against pure C# method calls.

To put this in relation to another similar library, read here: [Reply to please compare Routemesiter with MediatR](http://danielwertheim.se/reply-to-please-compare-routemesiter-with-mediatr/)

The C# variant will not really route a message. It knows where to call each time. That's why I included the `Routemeister manual Route` stats.

```
===== Pure C# - Shared handler =====
1,25546666666667ms / 100000calls
1,25546666666667E-05ms / call

===== Pure C# - New handler =====
1,45813333333333ms / 100000calls
1,45813333333333E-05ms / call

===== Routemeister - Shared handler =====
12,1029333333333ms / 100000calls
0,000121029333333333ms / call

===== Routemeister - New handler =====
12,1083333333333ms / 100000calls
0,000121083333333333ms / call

===== Routemeister manual Route - Shared handler =====
1,73613333333333ms / 100000calls
1,73613333333333E-05ms / call

===== Routemeister manual Route - New handler =====
1,67353333333333ms / 100000calls
1,67353333333333E-05ms / call
```
