using System;
using System.Data.Common;
using System.Data.Entity.Infrastructure;
using Glimpse.Ado.AlternateType;
using Glimpse.Core.Framework;

namespace Glimpse.EF.AlternateType
{
    public class GlimpseDbConnectionFactory : IDbConnectionFactory
    {
        public GlimpseDbConnectionFactory(IDbConnectionFactory inner, Func<IFrameworkProvider> getFrameworkProvider)
        {
            Inner = inner;
            GetFrameworkProvider = getFrameworkProvider;
        }

        private IDbConnectionFactory Inner { get; set; }

        private Func<IFrameworkProvider> GetFrameworkProvider { get; set; }

        public DbConnection CreateConnection(string nameOrConnectionString)
        {
            var connection = Inner.CreateConnection(nameOrConnectionString);
            var glimpseConnection = connection as GlimpseDbConnection;
            if (GetFrameworkProvider().CanPerformGlimpseWork)
            {
                return glimpseConnection == null ? new GlimpseDbConnection(connection) : connection;
            }
            else
            {
                return glimpseConnection == null ? connection : glimpseConnection.InnerConnection;
            }
        }
    }
}
