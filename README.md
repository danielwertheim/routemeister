# Routemeister
Routemeister is a small NuGet built with one single purpose. **Effectively performing in-process async message routing.** It can be used if you e.g. are dispatching messages cross process using RabbitMQ or ActiveMQ and then want to dispatch the message to typed handlers within the consuming process.

## Numbers
Below are some numbers comparing against pure C# method calls.

It's not really a fair comparision. Since the C# variant will not really route a message. It knows where to call each time. That's why I included the `Routemeister manual Route` stats.

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

## Usage
First, install it. It's distributed [via NuGet](https://www.nuget.org/packages/Routemeister).

```
install-package routemeister
```

Now, you need to **define a custom message handler marker interface**. The requirements are:

- Generic interface, with one generic argument (name it what ever you want)
- Should contain a single method (name it what ever you want)
- The method should accept one argument only (the message)
- The method should return `Task`.

```csharp
public interface IHandle<T>
{
    Task HandleAsync(T message);
}
```

Now, define some actual handlers (classes implementing your interface) that is supposed to process your messages. Each class can naturally implement the interface multiple times to handle different types of messages. Each message type being processed can (if you want) be processed by multiple message handlers (different classes).

```csharp
public class MyHandler :
    IHandle<MyConcreteMessage>,
    IHandle<ISomeInterfaceMessage>
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
    IHandle<MyConcreteMessage>,
    IHandle<ISomeInterfaceMessage>
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

Now, lets get the routes constructed. A route is represented by `MessageRoute`. Each message route has a `MessageType` and one-to-many `Actions`. Each `Action` represents a message handler.

To get the message routes, you use the `MessageRouteFactory`. To the factory, you pass a `Func<Type, object>` that is responsible for creating an instance of the message handler class. Why? Well, you could pass `Activator.CreateInstance` but you probably want to use your IoC container or something, so that additional dependencies are resolved via the container.

```csharp
//Using Activator
var factory = new MessageRouteFactory(type => Activator.CreateInstance(type));

//Using Ninject
var factory = new MessageRouteFactory(type => kernel.Get(type));
```

The factory needs to know in what assemblies to look for message handlers. It also needs to know which interface is used as the marker interface.

```csharp
MessageRoutes routes = factory.Create(typeof(SomeType).Assembly, typeof(IHandle<>));
```

The `routes` can now be used as you want to route messages manually, or you can start using an existing router, e.g. the `SequentialAsyncMessageRouter`.

```csharp
var router = new SequentialAsyncMessageRouter(routes);
await router.RouteAsync(new MyConcreteMessage
{
    //Some data
}).ConfigureAwait(false);
```

or manually

```csharp
var message = new MyConcreteMessage
{
    //Some data
};
var messageType = message.GetType();
var route = routes.GetRoute(messageType);
foreach (var action in route.Actions)
    await action(message).ConfigureAwait(false);
```
