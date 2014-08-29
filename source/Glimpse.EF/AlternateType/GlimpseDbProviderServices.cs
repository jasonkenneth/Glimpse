using System;
using System.Data.Common;
using System.Reflection;
#if EF43 || EF5
    using System.Data.Common.CommandTrees;
    using System.Data.Metadata.Edm;  
#else
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Metadata.Edm;
    using DbCommand = System.Data.Common.DbCommand;
    using DbConnection = System.Data.Common.DbConnection;
    using System.Data.Entity.Spatial;
#endif
#if EF5 && NET45
    using System.Data.Spatial;
#endif
using Glimpse.Ado.AlternateType;
using Glimpse.Core.Framework;

namespace Glimpse.EF.AlternateType
{
    internal class GlimpseDbProviderServices : DbProviderServices
    {
#if (EF5 && NET45) || EF6
        private readonly MethodInfo setParameterValueMethod;
#endif
        public GlimpseDbProviderServices(DbProviderServices innerProviderServices, Func<IFrameworkProvider> getFrameworkProvider)
        {
            InnerProviderServices = innerProviderServices;
            GetFrameworkProvider = getFrameworkProvider;

#if (EF5 && NET45) || EF6
            setParameterValueMethod = InnerProviderServices.GetType().GetMethod("SetParameterValue", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
#endif
        }

        private DbProviderServices InnerProviderServices { get; set; }

        private Func<IFrameworkProvider> GetFrameworkProvider { get; set; }

        public override DbCommandDefinition CreateCommandDefinition(DbCommand prototype)
        {
            var commandDef = InnerProviderServices.CreateCommandDefinition(prototype);
            return GetFrameworkProvider().CanPerformGlimpseWork
                ? new GlimpseDbCommandDefinition(commandDef)
                : commandDef;
        }

        protected override DbCommandDefinition CreateDbCommandDefinition(DbProviderManifest providerManifest, DbCommandTree commandTree)
        {
            var commandDef = InnerProviderServices.CreateCommandDefinition(providerManifest, commandTree);
            return GetFrameworkProvider().CanPerformGlimpseWork
                ? new GlimpseDbCommandDefinition(commandDef)
                : commandDef;
        }

        protected override void DbCreateDatabase(DbConnection connection, int? commandTimeout, StoreItemCollection storeItemCollection)
        {
            var glimpseConnection = connection as GlimpseDbConnection;
            DbConnection rawConnection = glimpseConnection == null ? connection : glimpseConnection.InnerConnection;
            InnerProviderServices.CreateDatabase(rawConnection, commandTimeout, storeItemCollection);
        }

        protected override string DbCreateDatabaseScript(string providerManifestToken, StoreItemCollection storeItemCollection)
        {
            return InnerProviderServices.CreateDatabaseScript(providerManifestToken, storeItemCollection);
        }

        protected override bool DbDatabaseExists(DbConnection connection, int? commandTimeout, StoreItemCollection storeItemCollection)
        {
            var glimpseConnection = connection as GlimpseDbConnection;
            DbConnection rawConnection = glimpseConnection == null ? connection : glimpseConnection.InnerConnection;
            return InnerProviderServices.DatabaseExists(rawConnection, commandTimeout, storeItemCollection);
        }

        protected override void DbDeleteDatabase(DbConnection connection, int? commandTimeout, StoreItemCollection storeItemCollection)
        {
            var glimpseConnection = connection as GlimpseDbConnection;
            DbConnection rawConnection = glimpseConnection == null ? connection : glimpseConnection.InnerConnection;
            InnerProviderServices.DeleteDatabase(rawConnection, commandTimeout, storeItemCollection);
        }

        protected override DbProviderManifest GetDbProviderManifest(string manifestToken)
        {
            return InnerProviderServices.GetProviderManifest(manifestToken);
        }

        protected override string GetDbProviderManifestToken(DbConnection connection)
        {
            var glimpseConnection = connection as GlimpseDbConnection;
            DbConnection rawConnection = glimpseConnection == null ? connection : glimpseConnection.InnerConnection;
            return InnerProviderServices.GetProviderManifestToken(rawConnection);
        }

#if (EF5 && NET45) || EF6Plus
        protected override DbSpatialDataReader GetDbSpatialDataReader(DbDataReader reader, string manifestToken)
        {
            var glimpseReader = reader as GlimpseDbDataReader;
            DbDataReader rawReader = glimpseReader == null ? reader : glimpseReader.InnerDataReader;
            return InnerProviderServices.GetSpatialDataReader(rawReader, manifestToken);
        }
#endif

#if (EF5 && NET45) || EF6
        // SetParameterValue is internal and am unable to call it on the InnerProviderServices from here. 
        // This breaks the provider wrapper when making spatial queries in EF 6.0.1
        // http://stackoverflow.com/questions/19966106/spatial-datareader-and-wrapping-providers-in-ef6  
        protected override void SetDbParameterValue(DbParameter parameter, TypeUsage parameterType, object value)
        { 
            setParameterValueMethod.Invoke(InnerProviderServices, new[] { parameter, parameterType, value });
        }
#endif

#if EF7Plus
        protected override void SetDbParameterValue(DbParameter parameter, TypeUsage parameterType, object value)
        {
            InnerProviderServices.SetParameterValue(parameter, parameterType, value);
        } 
#endif
    }
}