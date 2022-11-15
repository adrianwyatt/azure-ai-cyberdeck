using cogdeck.Handlers;
using Microsoft.Extensions.Hosting;

namespace cogdeck.HostedServices
{
    internal class ScreenRenderService : IHostedService
    {
        private Task _task;
        private readonly CancellationTokenSource _cancellationTokenSource = new();

        public ScreenRenderService(IEnumerable<IHandler> handlers)
        {

        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _task = ExecuteAsync(_cancellationTokenSource.Token);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _cancellationTokenSource.Cancel();
            return _task;
        }

        private Task ExecuteAsync(CancellationToken cancellationToken)
        {
            // Render left menu

            // Render right workspace
            // Render log output

            return Task.CompletedTask;
        }

    }
}
