using CarService.Proto.Car;
using CouchDB.Driver.Types;
using Newtonsoft.Json;

namespace CarService.DataConnector.DbContext
{
    [JsonObject("car")]
    public partial class CarDocument : CouchDocument
    {
        public Car Car { get; set; }
    }
}
