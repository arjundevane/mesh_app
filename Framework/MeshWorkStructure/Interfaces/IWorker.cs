using Google.Protobuf;

namespace MeshApp.WorkStructure.Interfaces
{
    /// <summary>
    /// Implement this interface for the worker to be picked up by Orchestration logic on startup.
    /// Provide a custom intent name using the <see cref="FulfilsIntentAttribute"/>.
    /// Otherwise intent name defaults to the class name.
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    public interface IWorker<TRequest, TResponse>
        where TRequest : IMessage<TRequest>
        where TResponse : IMessage<TResponse>
    {
        /// <summary>
        /// Entry point to the work performer logic.
        /// Must accept <typeparamref name="TRequest"/> parameter and return <typeparamref name="TResponse"/>
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public Task<TResponse> RunAsync(TRequest request);
    }
}