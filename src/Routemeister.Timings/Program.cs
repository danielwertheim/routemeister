using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Routemeister.Dispatchers;
using Routemeister.Routers;

namespace Routemeister.Timings
{
    class Program
    {
        static void Main(string[] args)
        {
            const int numOfCalls = 100000;

            ///***** PURE C# *****/
            var handler = new SampleHandler();
            Time<Message>("Pure C# - Shared handler", numOfCalls, handler.HandleAsync);
            Time<Message>("Pure C# - New handler", numOfCalls, m => new SampleHandler().HandleAsync(m));

            /***** ROUTEMEISTER *****/
            var routeFactory = new MessageRouteFactory();
            var routes = routeFactory.Create(Assembly.GetExecutingAssembly(), typeof(IMyHandlerOf<>));
            var reqRoutes = routeFactory.Create(Assembly.GetExecutingAssembly(), typeof(IMyAsyncRequestHandlerOf<,>));
            var sharedHandlerRouter = new SequentialAsyncRouter((t, e) => handler, routes);
            var newHandlerRouter = new SequentialAsyncRouter((t, e) => new SampleHandler(), routes);
            var asyncDispatcherSharedHandler = new AsyncDispatcher((t, e) => handler, reqRoutes);
            var asyncDispatcherNewHandler = new AsyncDispatcher((t, e) => new SampleHandler(), reqRoutes);

            Time<Message>("SequentialAsyncRouter - Shared handler", numOfCalls, sharedHandlerRouter.RouteAsync);
            Time<Message>("SequentialAsyncRouter - New handler", numOfCalls, newHandlerRouter.RouteAsync);

            Time<MyRequest>("AsyncDispatcher.Send - Shared handler", numOfCalls, asyncDispatcherSharedHandler.SendAsync);
            Time<MyRequest>("AsyncDispatcher.Send - New handler", numOfCalls, asyncDispatcherNewHandler.SendAsync);

            Time<MyRequest>("AsyncDispatcher.Publish - Shared handler", numOfCalls, asyncDispatcherSharedHandler.PublishAsync);
            Time<MyRequest>("AsyncDispatcher.Publish - New handler", numOfCalls, asyncDispatcherNewHandler.PublishAsync);

            Time<MyRequest>("AsyncDispatcher.Request - Shared handler", numOfCalls, asyncDispatcherSharedHandler.RequestAsync);
            Time<MyRequest>("AsyncDispatcher.Request - New handler", numOfCalls, asyncDispatcherNewHandler.RequestAsync);

            var messageType = typeof(Message);
            var route = routes.GetRoute(messageType);
            var routeAction = route.Actions.Single();
            Time<Message>("Manual Route - Shared handler", numOfCalls, m => routeAction.Invoke(handler, m));
            Time<Message>("Manual Route - New handler", numOfCalls, m => routeAction.Invoke(new SampleHandler(), m));
        }

        private static async void Time<TMessage>(string testCase, int numOfCalls, Func<TMessage, Task> dispatch) where TMessage : new()
        {
            var stopWatch = new Stopwatch();
            var timings = new List<TimeSpan>();
            var message = new TMessage();

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

    public interface IMyAsyncRequestHandlerOf<in TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        Task<TResponse> HandleAsync(TRequest request);
    }

    public class SampleHandler :
        IMyHandlerOf<Message>,
        IMyAsyncRequestHandlerOf<MyRequest, MyResponse>
    {
        public Task HandleAsync(Message message)
        {
            return Task.FromResult(0);
        }

        public Task<MyResponse> HandleAsync(MyRequest request)
        {
            return Task.FromResult(new MyResponse
            {
                Value = request.Value + 100
            });
        }
    }

    public class MyRequest : IRequest<MyResponse>
    {
        public int Value { get; set; }
    }

    public class MyResponse
    {
        public int Value { get; set; }
    }
}
