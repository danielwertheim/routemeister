using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Routemeister.Routers;

namespace Routemeister.Timings
{
    class Program
    {
        static void Main(string[] args)
        {
            const int numOfCalls = 100000;

            /***** PURE C# *****/
            var handler = new SampleHandler();
            Time("Pure C# - Shared handler", numOfCalls, m => handler.HandleAsync(m));
            Time("Pure C# - New handler", numOfCalls, m => new SampleHandler().HandleAsync(m));

            /***** ROUTEMEISTER *****/
            var sharedHandlerFactory = new MessageRouteFactory(t => handler);
            var newHandlerFactory = new MessageRouteFactory(t => new SampleHandler());
            var sharedHandlerRoutes = sharedHandlerFactory.Create(Assembly.GetExecutingAssembly(), typeof(IMyHandlerOf<>));
            var newHandlerRoutes = newHandlerFactory.Create(Assembly.GetExecutingAssembly(), typeof(IMyHandlerOf<>));
            var sharedHandlerRouter = new SequentialAsyncMessageRouter(sharedHandlerRoutes);
            var newHandlerRouter = new SequentialAsyncMessageRouter(newHandlerRoutes);

            Time("Routemeister - Shared handler", numOfCalls, sharedHandlerRouter.RouteAsync);
            Time("Routemeister - New handler", numOfCalls, newHandlerRouter.RouteAsync);

            var sharedRoute = sharedHandlerRoutes.GetRoute(typeof(Message));
            var sharedRouteAction = sharedRoute.Actions.Single();
            Time("Routemeister manual Route - Shared handler", numOfCalls, m => sharedRouteAction(m));

            var route = sharedHandlerRoutes.GetRoute(typeof(Message));
            var routeAction = route.Actions.Single();
            Time("Routemeister manual Route - New handler", numOfCalls, m => routeAction(m));
        }

        private static async void Time(string testCase, int numOfCalls, Func<Message, Task> dispatch)
        {
            var stopWatch = new Stopwatch();
            var timings = new List<TimeSpan>();
            var message = new Message();

            for (var c = 0; c < 5; c++)
            {
                stopWatch.Start();
                for (var i = 0; i < numOfCalls; i++)
                {
                    await dispatch(message).ConfigureAwait(false);
                }
                stopWatch.Stop();
                timings.Add(stopWatch.Elapsed);
                stopWatch.Reset();
            }

            var sum = timings
                .Select(t => t.TotalMilliseconds)
                .OrderBy(ms => ms)
                .Skip(1)
                .Take(timings.Count - 2)
                .Sum();
            var avg = sum / (timings.Count - 2);

            Console.WriteLine($"===== {testCase} =====");
            Console.WriteLine($"{avg}ms / {numOfCalls}calls");
            Console.WriteLine($"{avg / numOfCalls}ms / call");
            Console.WriteLine();
        }
    }

    public class Message { }

    public interface IMyHandlerOf<in T>
    {
        Task HandleAsync(T message);
    }

    public class SampleHandler : IMyHandlerOf<Message>
    {
        public Task HandleAsync(Message message)
        {
            return Task.FromResult(0);
        }
    }
}
