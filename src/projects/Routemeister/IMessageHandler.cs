namespace Routemeister
{
    /// <summary>
    /// A premade definition of a synchronous handler.
    /// Please note, you can define your own interface for this.
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public interface IMessageHandler<in TMessage> where TMessage : class
    {
        void Handle(TMessage message);
    }
}