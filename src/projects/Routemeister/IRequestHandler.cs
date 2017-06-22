namespace Routemeister
{
    /// <summary>
    /// A premade definition of a synchronous request handler.
    /// Please note, you can define your own interface for this.
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    public interface IRequestHandler<in TRequest, out TResponse> where TRequest : IRequest<TResponse>
    {
        TResponse Handle(TRequest request);
    }
}