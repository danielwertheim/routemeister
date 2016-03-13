using System.Threading.Tasks;

namespace Routemeister
{
    internal delegate Task MessageHandlerInvoker(object messageHandlerContainer, object message);
}