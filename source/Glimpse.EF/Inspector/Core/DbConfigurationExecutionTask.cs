#if EF6Plus
using System;
using System.Data.Entity;
using System.Data.Entity.Core.Common;
using Glimpse.Core.Framework;
using Glimpse.Core.Framework.Support;
using Glimpse.EF.AlternateType;

namespace Glimpse.EF.Inspector.Core
{
    public class DbConfigurationExecutionTask : IExecutionTask
    {
        public DbConfigurationExecutionTask(Func<IFrameworkProvider> getFrameworkProvider)
        {
            GetFrameworkProvider = getFrameworkProvider;
        }

        private Func<IFrameworkProvider> GetFrameworkProvider { get; set; }

        public void Execute()
        {
            DbConfiguration.Loaded += (_, a) => 
                a.ReplaceService<DbProviderServices>((s, k) => 
                    s.GetType() == typeof(GlimpseDbProviderServices) ? s : new GlimpseDbProviderServices(s, GetFrameworkProvider));

            DbConfiguration.Loaded += (_, a) =>
                a.AddDependencyResolver(new InvariantNameResolver(), true);
        }
    }
}
#endif