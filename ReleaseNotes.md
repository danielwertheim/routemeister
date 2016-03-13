#Release notes

## v0.1.0 - 2016-02-21
First release.

## v0.2.0 - 2016-03-13
Now using `IL Emits` to get faster calls/routing.

The `IL Emits` optimizations allows for a change in resolving the instances of the classes handling the messages. **NO MORE SINGLETON**, unless you want to. You are in control. Now, the resolving function passed to the `MessageRouteFactory` is called when each message is routed. No more singleton behavior. Of course you can accomplish singleton by resolving the same instance each time. This is something you would handle in your IoC-container if that is hooked in.