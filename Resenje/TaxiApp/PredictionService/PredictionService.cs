using Common.Interfaces;
using Common.Models;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System.Fabric;

namespace PredictionService
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class PredictionService : StatelessService, IPredictionService
    {
        public PredictionService(StatelessServiceContext context)
            : base(context)
        { }

        public async Task<PredictionModel> GetPredictionPrice(string currentLocation, string destination)
        {
            double rangeMin = 5.0;
            double rangeMax = 20.0;

            Random r = new Random();
            double price = rangeMin + (rangeMax - rangeMin) * r.NextDouble();

            // Create TimeSpan objects
            TimeSpan estimatedTimeMin = new TimeSpan(0, 1, 0); // 1 minute
            TimeSpan estimatedTimeMax = new TimeSpan(0, 2, 0); // 2 minutes

            return new PredictionModel(price, estimatedTimeMin, estimatedTimeMax);

        }
        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
           => this.CreateServiceRemotingInstanceListeners(); //DODATO

        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following sample code with your own logic 
            //       or remove this RunAsync override if it's not needed in your service.

            long iterations = 0;

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                ServiceEventSource.Current.ServiceMessage(this.Context, "Working-{0}", ++iterations);

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }
    }
}
