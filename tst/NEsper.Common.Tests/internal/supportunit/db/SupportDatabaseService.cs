///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.client.db;
using com.espertech.esper.common.@internal.db;
using com.espertech.esper.common.@internal.epl.historical.database.connection;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.container;

namespace com.espertech.esper.common.@internal.supportunit.db
{
    public class SupportDatabaseService
	{
        private const string ESPER_LOCAL_CONFIG_FILE = "NEsperConfig.xml";

        private readonly IDriverResolver _driverResolver;

        public readonly ConfigurationCommonDBRef DbConfigReferenceNative;
        public readonly ConfigurationCommonDBRef DbConfigReferenceODBC;

	    public readonly DriverConnectionFactoryDesc DriverConnectionFactoryNative;
        public readonly DriverConnectionFactoryDesc DriverConnectionFactoryOdbc;

	    public DriverConnectionFactoryDesc DriverConnectionFactoryDefault { get; }

        private readonly IContainer _container;

        public static SupportDatabaseService GetInstance(IContainer container)
        {
            if (!container.Has<SupportDatabaseService>()) {
                RegisterSingleton(container);
            }
            return container.Resolve<SupportDatabaseService>();
        }

        public static void RegisterSingleton(IContainer container)
        {
            container.Register<SupportDatabaseService>(
                xx => new SupportDatabaseService(container),
                Lifespan.Singleton);
        }

        private SupportDatabaseService(IContainer container)
        {
            _container = container;
            _driverResolver = container.Resolve<IDriverResolver>();

            var configurationPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ESPER_LOCAL_CONFIG_FILE);
            var configurationFile = new FileInfo(configurationPath);
            var configuration = new ConfigurationCommon();
            var configurationDocument = new XmlDocument();
            configurationDocument.Load(configurationFile.FullName);
            ConfigurationCommonParser.DoConfigure(configuration, configurationDocument.DocumentElement);

            var dbTable = configuration.DatabaseReferences;

            DriverConnectionFactoryDefault = dbTable["db1"].ConnectionFactoryDesc as DriverConnectionFactoryDesc;

            DbConfigReferenceNative = dbTable["db1"];
            DbConfigReferenceODBC = dbTable["db2"];

            DriverConnectionFactoryNative = DbConfigReferenceNative
                .ConnectionFactoryDesc as DriverConnectionFactoryDesc;
            DriverConnectionFactoryOdbc = DbConfigReferenceODBC
                .ConnectionFactoryDesc as DriverConnectionFactoryDesc;
	    }

        /// <summary>
        /// Gets the first database driver.
        /// </summary>
        public DbDriver DriverNative => DbDriverConnectionHelper.ResolveDriver(_driverResolver, DriverConnectionFactoryNative);

        /// <summary>
        /// Gets the second database driver.
        /// </summary>
        public DbDriver DriverODBC => DbDriverConnectionHelper.ResolveDriver(_driverResolver, DriverConnectionFactoryOdbc);

        public const string DBNAME_FULL = "mydb";
        public const string DBNAME_PART = "mydb2";

	    public DatabaseConfigServiceImpl MakeService(ImportService importService)
		{
            IDictionary<string, ConfigurationCommonDBRef> mapDatabaseRef =
                new Dictionary<string, ConfigurationCommonDBRef>();

            mapDatabaseRef.Put(DBNAME_FULL, DbConfigReferenceNative);
            mapDatabaseRef.Put(DBNAME_PART, DbConfigReferenceODBC);

            var driverResolver = _container.Resolve<IDriverResolver>();
            return new DatabaseConfigServiceImpl(
                    driverResolver,
                    mapDatabaseRef,
                    importService);
		}

        public Properties DefaultProperties
        {
            get
            {
                var serverHost = Environment.GetEnvironmentVariable("ESPER_SQL_HOST");
                var serverUser = Environment.GetEnvironmentVariable("ESPER_SQL_USER");
                var serverPass = Environment.GetEnvironmentVariable("ESPER_SQL_PASSWORD");
                var serverDbase = Environment.GetEnvironmentVariable("ESPER_SQL_DBASE");

                if (serverHost == null)
                    serverHost = "localhost";
                if (serverUser == null)
                    serverUser = "esper";
                if (serverPass == null)
                    serverPass = "3sp3rP@ssw0rd";
                if (serverDbase == null)
                    serverDbase = "esper";

                var properties = new Properties();
                properties["Server"] = serverHost;
                properties["Uid"] = serverUser;
                properties["Pwd"] = serverPass;
                properties["Database"] = serverDbase;

                return properties;
            }
        }

	    public ConfigurationCommonDBRef CreateDefaultConfig(Properties properties = null)
	    {
	        var configDB = new ConfigurationCommonDBRef();
	        configDB.SetDatabaseDriver(DriverConnectionFactoryDefault.Merge(properties));
	        return configDB;
	    }
	}
}
