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
            Time("Pure C# - Shared handler", numOfCalls, m => handler.HandleAsync(m.Message as Message));
            Time("Pure C# - New handler", numOfCalls, m => new SampleHandler().HandleAsync(m.Message as Message));

            /***** ROUTEMEISTER *****/
            var routeFactory = new MessageRouteFactory();
            var routes = routeFactory.Create(Assembly.GetExecutingAssembly(), typeof (IMyHandlerOf<>));
            var sharedHandlerRouter = new SequentialAsyncMessageRouter((t, e) => handler, routes);
            var newHandlerRouter = new SequentialAsyncMessageRouter((t, e) => new SampleHandler(), routes);

            Time("Routemeister - Shared handler", numOfCalls, sharedHandlerRouter.RouteAsync);
            Time("Routemeister - New handler", numOfCalls, newHandlerRouter.RouteAsync);

            var messageType = typeof(Message);
            var route = routes.GetRoute(messageType);
            var routeAction = route.Actions.Single();
            Time("Routemeister manual Route - Shared handler", numOfCalls, envelope => routeAction.Invoke(handler, envelope.Message));
            Time("Routemeister manual Route - New handler", numOfCalls, envelope => routeAction.Invoke(new SampleHandler(), envelope.Message));
        }

        private static async void Time(string testCase, int numOfCalls, Func<MessageEnvelope, Task> dispatch)
        {
            var stopWatch = new Stopwatch();
            var timings = new List<TimeSpan>();
            var message = new Message();
            var envelope = new MessageEnvelope(message, message.GetType());

            for (var c = 0; c < 5; c++)
            {
                stopWatch.Start();
                for (var i = 0; i < numOfCalls; i++)
                {
                    await dispatch(envelope).ConfigureAwait(false);
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
