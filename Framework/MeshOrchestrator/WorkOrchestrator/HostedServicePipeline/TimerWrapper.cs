using MeshApp.WorkOrchestrator.Services;
using Timer = System.Timers.Timer;

namespace MeshApp.WorkOrchestrator.HostedServicePipeline
{
    public class TimerWrapper : IHostedService
    {
        private readonly Timer _registrationTimer;
        private readonly Timer _intentResolverTimer;
        private const double _cRegistrationInterval = 10000; // Timer interval in milliseconds
        private const double _cIntentResolverInterval = 60000; // Timer interval in milliseconds
        private readonly IRegistration _registration;
        private readonly IIntentResolverBuilder _intentResolver;

        public TimerWrapper(IRegistration registration, IIntentResolverBuilder intentResolver)
        {
            _registration = registration;
            _registrationTimer = new Timer();
            _registrationTimer.Interval = _cRegistrationInterval;

            _intentResolver = intentResolver;
            _intentResolverTimer = new Timer();
            _intentResolverTimer.Interval = _cIntentResolverInterval;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _registrationTimer.Start();
            _registrationTimer.Elapsed += _registration.CheckWorkerRegistrations;

            _intentResolverTimer.Start();
            _intentResolverTimer.Elapsed += _intentResolver.TimerTriggerBuildIntentResolvers;

            // Manually invoke the intent resolve for first time to initialize intents.
            _intentResolver.ManualTriggerBuildIntentResolvers();

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _registrationTimer.Stop();
            _intentResolverTimer.Stop();

            return Task.CompletedTask;
        }
    }
}
