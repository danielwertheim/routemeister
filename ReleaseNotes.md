#Release notes

## v1.0.0 - 2016-03-22
With this rewrite, the API is now considered stable and no more breaking changes are planned.

- **[Breaking]:** Renamed: `MiddlewareEnabledSequentialAsyncMessageRouter` to `MiddlewareEnabledAsyncMessageRouter`.
- **[Breaking]:** The `MessageRouteFactory` does not depend on a `MessageHandlerCreator` anymore. The creator is instead injected to any of the pre-made routers.
- **[Breaking]:** The routers now accept a `MessageHandlerCreator`. This is where you inject your resolving IoC-container.
- **[Breaking]:** A `MessageRoute` now expose `IMessageHandlerAction[]` instead of `Func<MessageEnvelope, Task>`

## v0.3.0 - 2016-03-18
- **[Breaking]:** The resolver that you pass to the `MessageRouteFactory` is now a delegate `MessageHandlerCreator` with a signature `object MessageHandlerCreator(Type messageHandlerContainerType, MessageEnvelope envelope)`. More info in the README.md
- **[New]**: Internally routhing is using a new `MessageEnvelope`. This allows for e.g. hooking in state on a certain message being routed.
- **[New]**: New pre-made router: `MiddlewareEnabledSequentialAsyncMessageRouter`; which allows you to hook into the routing pipe. Similar to Owin. `router.Use(next => async envelope => {...});`. More info in the README.md

## v0.2.0 - 2016-03-13
Now using `IL Emits` to get faster calls/routing.

The `IL Emits` optimizations allows for a change in resolving the instances of the classes handling the messages. **NO MORE SINGLETON**, unless you want to. You are in control. Now, the resolving function passed to the `MessageRouteFactory` is called when each message is routed. No more singleton behavior. Of course you can accomplish singleton by resolving the same instance each time. This is something you would handle in your IoC-container if that is hooked in.

## v0.1.0 - 2016-02-21
First release.