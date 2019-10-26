using MongoDB.Driver;
using System.Threading.Tasks;

namespace Hemma.TelldusLogger
{
    public interface ITelldusRepository
    {
        Task Save(TelldusTemperatureDatapoint entity);
    }

    public class TelldusRepository : ITelldusRepository
    {
        public async Task Save(TelldusTemperatureDatapoint entity)
        {
            var mongoclient = new MongoClient(new MongoUrl("mongodb://hemmaserver2"));
            var database = mongoclient.GetDatabase("Telldus");

            var datas = database.GetCollection<TelldusTemperatureDatapoint>("LoggedData");

            var filter = "{ Timestamp: " + entity.Timestamp + "}";

            var existingItem = await datas.FindAsync<TelldusTemperatureDatapoint>(filter);
            if (existingItem.FirstOrDefault() == null)
                datas.InsertOne(entity);

        }
    }
}