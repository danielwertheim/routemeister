# Routemeister
Routemeister is build for assisting with in-process async message routing.

## Usage
First, install it. It's distributed via NuGet.

```
install-package routemeister
```

Now, you need to define a message handler marker interface. The requirements are:

- Generic interface (name it what ever you want) with one generic argument
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

Know, the `routes` can be used as you want to route messages manually or using e.g. `SequentialAsyncMessageRouter`.

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