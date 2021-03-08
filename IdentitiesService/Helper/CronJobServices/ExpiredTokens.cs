using IdentitiesService.Helper.Abstraction;
using IdentitiesService.Helper.CronJobServices.CronJobExtensionMethods;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace IdentitiesService.Helper.CronJobServices
{
    public class ExpiredTokens : CronJobService, IDisposable
    {
        private readonly IServiceScope _scope;
        public ExpiredTokens(IScheduleConfig<ExpiredTokens> config, IServiceProvider scope) : base(config.CronExpression, config.TimeZoneInfo)
        {
            _scope = scope.CreateScope();
        }
        public override Task StartAsync(CancellationToken cancellationToken)
        {
            return base.StartAsync(cancellationToken);
        }
        public override Task DoWork(CancellationToken cancellationToken)
        {
            IHelperRepository _helperRepo = _scope.ServiceProvider.GetRequiredService<IHelperRepository>();
            try
            {
                _helperRepo.RemoveExpiredTokens();
            }
            catch (Exception) { }
            return Task.CompletedTask;
        }
        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return base.StopAsync(cancellationToken);
        }
        public override void Dispose()
        {
            _scope?.Dispose();
        }
    }
}
