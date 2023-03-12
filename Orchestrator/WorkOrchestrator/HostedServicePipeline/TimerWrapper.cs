using WorkOrchestrator.Registration;
using Timer = System.Timers.Timer;

namespace WorkOrchestrator.HostedServicePipeline
{
    public class TimerWrapper : IHostedService
    {
        private readonly Timer _timer;
        private const double _cInternal = 10000; // Timer interval in milliseconds
        private readonly IRegistration _registration;

        public TimerWrapper(IRegistration registration)
        {
            _registration = registration;
            _timer = new Timer();
            _timer.Interval = _cInternal;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer.Start();
            _timer.Elapsed += _registration.CheckWorkerRegistrations;
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer.Stop();

            return Task.CompletedTask;
        }
    }
}
