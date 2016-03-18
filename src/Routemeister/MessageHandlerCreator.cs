using System;

namespace Routemeister
{
    public delegate object MessageHandlerCreator(Type messageHandlerContainerType, MessageEnvelope envelope);
}