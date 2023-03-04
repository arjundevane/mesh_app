namespace MeshApp.WorkInterface
{
    public interface IWorker<TRequest, TResponse>
    {
        public Task<TResponse> RunAsync(TRequest request);
    }
}