using CarService.Proto.Car;
using MeshApp.WorkStructure.Interfaces;

namespace CarService.WorkerImplementation.FindMostEfficientInListWorker
{
    [FulfilsIntent("FindMostEfficientCarByMakeEnd")]
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