using System.Threading.Tasks;

namespace Routemeister
{
    /// <summary>
    /// A premade definition of an async handler.
    /// Please note, you can define your own interface for this.
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public interface IAsyncMessageHandler<in TMessage> where TMessage : class
    {
        Task HandleAsync(TMessage message);
    }
}