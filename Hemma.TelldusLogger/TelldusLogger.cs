using System.Threading.Tasks;

namespace Hemma.TelldusLogger
{
    public interface ITelldusLogger
    {
        Task Run();
    }

    public class TelldusLogger : ITelldusLogger
    {
        private readonly IDataFetcher dataFetcher;
        private readonly ITelldusTemperatureFactory telldusTemperatureFactory;
        private readonly ITelldusRepository telldusRepository;

        public TelldusLogger(IDataFetcher dataFetcher,
            ITelldusTemperatureFactory telldusTemperatureFactory,
            ITelldusRepository telldusRepository)
        {
            this.dataFetcher = dataFetcher;
            this.telldusTemperatureFactory = telldusTemperatureFactory;
            this.telldusRepository = telldusRepository;
        }
        public async Task Run()
        {
            var data = await this.dataFetcher.GetDatapoint("1530753086");
            var datapoint = this.telldusTemperatureFactory.Create(data);
            this.telldusRepository.Save(datapoint);
        }
    }
}