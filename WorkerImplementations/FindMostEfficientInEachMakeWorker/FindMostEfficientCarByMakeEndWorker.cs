using MeshApp.Proto;
using MeshApp.WorkInterface;

namespace FindMostEfficientInListWorker
{
    public class FindMostEfficientCarByMakeEndWorker : IWorker<GetCarEfficiencyByMakeResponse, FindMostEfficientCarByMakeResponse>
    {
        public Task<FindMostEfficientCarByMakeResponse> RunAsync(GetCarEfficiencyByMakeResponse request)
        {
            var allCars = request.Efficiencies;

            var mostEfficient = allCars
                                .OrderByDescending(c => c.HorsepowerPerCcDisplacement)
                                .Select(c => c.Car)
                                .FirstOrDefault();

            return Task.FromResult(new FindMostEfficientCarByMakeResponse
            {
                MostEfficientCar = mostEfficient
            });
        }
    }
}