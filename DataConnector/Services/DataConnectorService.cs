using CouchDB.Driver.Extensions;
using Grpc.Core;
using MeshApp.DataConnector.DbContext;
using MeshApp.Error;
using MeshApp.Proto;

namespace DataConnector.Services
{
    public class DataConnectorService : CarService.CarServiceBase
    {
        private readonly ILogger<DataConnectorService> _logger;
        private readonly DataConnectorContext _context;

        public DataConnectorService(ILogger<DataConnectorService> logger, DataConnectorContext dbContext)
        {
            _logger = logger;
            _context = dbContext;
        }

        public async override Task<CarResponse> GetCar(FindCarByIdRequest request, ServerCallContext context)
        {
            try
            {
                var cars = await _context.Cars
                .Where(c => c.Car.CarId == request.CarId)
                .ToListAsync();

                var response = new CarResponse();
                response.Cars.AddRange(cars.Select(c => c.Car));

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Detected error while processing: {request}");
                return new CarResponse
                {
                    Error = new Error
                    {
                        ErrorMessage = ex.Message,
                        StackTrace = ex.StackTrace
                    }
                };
            }
        }
    }
}
