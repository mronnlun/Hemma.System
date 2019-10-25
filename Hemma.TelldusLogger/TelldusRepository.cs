using MongoDB.Driver;

namespace Hemma.TelldusLogger
{
    public interface ITelldusRepository
    {
        void Save(TelldusTemperatureDatapoint entity);
    }

    public class TelldusRepository : ITelldusRepository
    {
        public void Save(TelldusTemperatureDatapoint entity)
        {
            var mongoclient = new MongoClient(new MongoUrl("mongodb://hemmaserver2"));
            var database = mongoclient.GetDatabase("Telldus");

            var datas = database.GetCollection<TelldusTemperatureDatapoint>("LoggedData");
            datas.InsertOne(entity);

        }
    }
}