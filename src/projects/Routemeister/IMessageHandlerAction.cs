using System;
using System.Threading.Tasks;

namespace Routemeister
{
    public interface IMessageHandlerAction
    {
        Type HandlerType { get; }
        Type MessageType { get; }

        Task Invoke(object handler, object message);
    }
}