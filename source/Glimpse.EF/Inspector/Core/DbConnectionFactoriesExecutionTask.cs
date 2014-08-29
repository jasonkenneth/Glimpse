using System;
using System.Data.Entity;
using System.Reflection;
using Glimpse.Core.Extensibility;
using Glimpse.Core.Framework;
using Glimpse.Core.Framework.Support;
using Glimpse.EF.AlternateType;

namespace Glimpse.EF.Inspector.Core
{
    public class DbConnectionFactoriesExecutionTask : IExecutionTask
    {
        public DbConnectionFactoriesExecutionTask(ILogger logger, Func<IFrameworkProvider> getFrameworkProvider)
        {
            Logger = logger;
            GetFrameworkProvider = getFrameworkProvider;
        }

        private ILogger Logger { get; set; }

        private Func<IFrameworkProvider> GetFrameworkProvider { get; set; }

        public void Execute()
        {
            Logger.Info("EntityFrameworkInspector: Starting to replace DefaultConnectionFactory");

            var databaseType = typeof(Database);
            var defaultConnectionFactoryChanged = (bool)databaseType.GetProperty("DefaultConnectionFactoryChanged", BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.NonPublic).GetValue(databaseType, null);

            if (defaultConnectionFactoryChanged)
            {
                Logger.Info("EntityFrameworkInspector: Detected that user is using a custom DefaultConnectionFactory");

                Database.DefaultConnectionFactory = new GlimpseDbConnectionFactory(Database.DefaultConnectionFactory, GetFrameworkProvider);
            }

            Logger.Info("EntityFrameworkInspector: Finished to replacing DefaultConnectionFactory");
        }
    }
}