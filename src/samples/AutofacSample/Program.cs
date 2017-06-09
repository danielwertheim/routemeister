using System;
using System.Threading.Tasks;
using Autofac;
using Routemeister;
using Routemeister.Dispatchers;
using Routemeister.Routers;

namespace AutofacSample
{
    class Program
    {
        static void Main(string[] args)
        {
            var builder = new ContainerBuilder();
            builder.RegisterAssemblyModules(typeof(Program).Assembly);
            var container = builder.Build();

            Console.WriteLine($"===== Using {nameof(MiddlewareEnabledAsyncRouter)} =====");
            var router1 = container.Resolve<MiddlewareEnabledAsyncRouter>();
            router1.RouteAsync(new MyMessage()).Wait();
            router1.RouteAsync(new MyMessage()).Wait();

            Console.WriteLine($"===== Using {nameof(SequentialAsyncRouter)} =====");
            var router2 = container.Resolve<SequentialAsyncRouter>();
            router2.RouteAsync(new MyMessage()).Wait();
            router2.RouteAsync(new MyMessage()).Wait();

            Console.WriteLine($"===== Using {nameof(AsyncDispatcher)} =====");
            var dispatcher = container.Resolve<AsyncDispatcher>();
            dispatcher.SendAsync(new MyMessage()).Wait();
            dispatcher.SendAsync(new MyMessage()).Wait();
        }
    }

    public class MyMessage { }

    public class ConcreteHandler :
        IAsyncMessageHandler<MyMessage>,
        IDisposable
    {
        private readonly SomeDependency _dependency;

        public ConcreteHandler(SomeDependency dependency)
        {
            _dependency = dependency;
        }

        public async Task HandleAsync(MyMessage message)
        {
            await _dependency.DoWorkAsync();
        }

        public void Dispose()
        {
            Console.WriteLine("Concrete handler is being released");
        }
    }

    public class SomeDependency : IDisposable
    {
        public Task DoWorkAsync()
        {
            Console.WriteLine("Injected dependency doing work...");
            return Task.FromResult(0);
        }

        public void Dispose()
        {
            Console.WriteLine("Injected dependency being released.");
        }
    }

    internal static class MessageEnvelopeExtensions
    {
        internal static void SetScope(this MessageEnvelope envelope, ILifetimeScope scope)
        {
            envelope.SetState("scope", scope);
        }

        internal static ILifetimeScope GetScope(this MessageEnvelope envelope)
        {
            return envelope.GetState("scope") as ILifetimeScope;
        }
    }

    public class OurModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            //MessageRoutes
            builder.Register(ctx =>
                {
                    var factory = new MessageRouteFactory();

                    return factory.Create(
                        typeof(Program).Assembly,
                        typeof(IAsyncMessageHandler<>));
                })
                .SingleInstance();

            //MiddlewareEnabledAsyncRouter sample
            builder.Register(ctx =>
                {
                    var routes = ctx.Resolve<MessageRoutes>();
                    var parentScope = ctx.Resolve<ILifetimeScope>();
                    var router = new MiddlewareEnabledAsyncRouter(
                        (type, envelope) => envelope.GetScope().Resolve(type),
                        routes);
                    router.Use(next => envelope =>
                    {
                        using (var childScope = parentScope.BeginLifetimeScope())
                        {
                            envelope.SetScope(childScope);
                            return next(envelope);
                        }
                    });

                    return router;
                })
                .SingleInstance();

            //SequentialAsyncRouter sample
            builder.Register(ctx =>
                {
                    var routes = ctx.Resolve<MessageRoutes>();
                    var parentScope = ctx.Resolve<ILifetimeScope>();
                    var router = new SequentialAsyncRouter(
                        (type, envelope) => envelope.GetScope().Resolve(type),
                        routes)
                    {
                        OnBeforeRouting = envelope => envelope.SetScope(parentScope.BeginLifetimeScope()),
                        OnAfterRouted = envelope => envelope.GetScope()?.Dispose()
                    };


                    return router;
                })
                .SingleInstance();

            //AsyncDispatcher sample
            builder.Register(ctx =>
                {
                    var routes = ctx.Resolve<MessageRoutes>();
                    var parentScope = ctx.Resolve<ILifetimeScope>();
                    var router = new AsyncDispatcher(
                        (type, envelope) => envelope.GetScope().Resolve(type),
                        routes)
                    {
                        OnBeforeRouting = envelope => envelope.SetScope(parentScope.BeginLifetimeScope()),
                        OnAfterRouted = envelope => envelope.GetScope()?.Dispose()
                    };


                    return router;
                })
                .SingleInstance();

            builder.RegisterType<SomeDependency>();
            builder.RegisterType<ConcreteHandler>();
        }
    }
}