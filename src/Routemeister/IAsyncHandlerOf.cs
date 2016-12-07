using System.Threading.Tasks;

namespace Routemeister
{
    /// <summary>
    /// One definition of a handler. Please note that you can define your own.
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public interface IAsyncHandlerOf<in TMessage> where TMessage : class
    {
        Task HandleAsync(TMessage message);
    }
}