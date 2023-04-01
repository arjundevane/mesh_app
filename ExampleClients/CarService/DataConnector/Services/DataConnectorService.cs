using CarService.DataConnector.DbContext;
using CarService.Proto.Car;
using CouchDB.Driver.Extensions;
using Grpc.Core;

namespace CarService.DataConnector.Services
{
    public class DataConnectorService : Proto.Car.CarService.CarServiceBase
    {
        private readonly ILogger<DataConnectorService> _logger;
        private readonly DataConnectorContext _context;

        public DataConnectorService(ILogger<DataConnectorService> logger, DataConnectorContext dbContext)
        {
            _logger = logger;
            _context = dbContext;
        }

        public async override Task<CarsResponse> FindCarsById(FindCarByIdRequest request, ServerCallContext context)
        {
            try
            {
                var cars = await _context.Cars
                            .Where(c => c.Car.CarId == request.CarId.ToLower())
                            .ToListAsync();

                var response = new CarsResponse();
                response.Cars.AddRange(cars.Select(c => c.Car));

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Detected error while processing: {request}");
                return new CarsResponse { Error = GenerateErrorResponse(ex) };
            }
        }

        public async override Task<CarsResponse> FindCarsByMake(FindCarsByMakeRequest request, ServerCallContext context)
        {
            try
            {
                var cars = await _context.Cars
                            .Where(c => c.Car.Make == request.Make.ToLower())
                            .ToListAsync();

                var response = new CarsResponse();
                response.Cars.AddRange(cars.Select(c => c.Car));

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Detected error while processing: {request}");
                return new CarsResponse { Error = GenerateErrorResponse(ex) };
            }
        }

        public async override Task<CarResponse> AddCar(AddCarRequest request, ServerCallContext context)
        {
            try
            {
                var newCar = new CarDocument
                {
                    Car = new Car
                    {
                        CarId = Guid.NewGuid().ToString().ToLower(),
                        Engine = new Car.Types.Engine
                        {
                            Cylinders = request.AddCar.Engine.Cylinders,
                            Displacement = request.AddCar.Engine.Displacement,
                            Horsepower = request.AddCar.Engine.Horsepower
                        },
                        Make = request.AddCar.Make.ToLower(),
                        Name = request.AddCar.Name.ToLower(),
                        Year = request.AddCar.Year
                    }
                };

                await _context.Cars.AddAsync(newCar);

                var response = new CarResponse();
                response.Car = newCar.Car;

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Detected error while processing: {request}");
                return new CarResponse { Error = GenerateErrorResponse(ex) };
            }
        }

        private Error GenerateErrorResponse(Exception exception)
        {
            return new Error
            {
                ErrorMessage = exception.Message,
                StackTrace = exception.StackTrace
            };
        }
    }
}
