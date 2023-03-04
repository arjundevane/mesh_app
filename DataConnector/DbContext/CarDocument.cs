using CouchDB.Driver.Types;
using MeshApp.Proto;
using Newtonsoft.Json;

namespace DataConnector.DbContext
{
    [JsonObject("car")]
    public partial class CarDocument : CouchDocument
    {
        public Car Car { get; set; }
    }
}
