///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.support;
using com.espertech.esper.epl.db;
using com.espertech.esper.supportregression.util;

namespace com.espertech.esper.supportregression.epl
{
    using Configuration = esper.client.Configuration;
    
    public class SupportDatabaseService
	{
        protected internal const String ESPER_TEST_CONFIG = "regression/esper.test.readconfig.cfg.xml";

        public static readonly ConfigurationDBRef DbConfigReferenceNative;
        public static readonly ConfigurationDBRef DbConfigReferenceODBC;

	    public static readonly DbDriverFactoryConnection DbDriverFactoryNative;
        public static readonly DbDriverFactoryConnection DbDriverFactoryODBC;

	    public static DbDriverFactoryConnection DbDriverFactoryDefault { get; private set; }

	    private const string ESPER_REGRESSION_CONFIG_FILE = "NEsperRegressionConfig.xml";

	    static SupportDatabaseService()
	    {
	        var configurationFile = new FileInfo(ESPER_REGRESSION_CONFIG_FILE);
	        var configuration = new Configuration(SupportContainer.Instance);
	        configuration.Configure(configurationFile);

            var dbTable = configuration.DatabaseReferences;

            DbDriverFactoryDefault = dbTable["db1"].ConnectionFactoryDesc as DbDriverFactoryConnection;

            DbConfigReferenceNative = dbTable["db1"];
            DbConfigReferenceODBC = dbTable["db2"];

            DbDriverFactoryNative = DbConfigReferenceNative
                .ConnectionFactoryDesc as DbDriverFactoryConnection;
            DbDriverFactoryODBC = DbConfigReferenceODBC
                .ConnectionFactoryDesc as DbDriverFactoryConnection;
	    }

        /// <summary>
        /// Gets the first database driver.
        /// </summary>
        public static DbDriver DriverNative => DbDriverFactoryNative.Driver;

	    /// <summary>
        /// Gets the second database driver.
        /// </summary>
        public static DbDriver DriverODBC => DbDriverFactoryODBC.Driver;

	    public const string PGSQLDB_PROVIDER_TYPE = "Npgsql.NpgsqlFactory";

        public const String DBNAME_FULL = "mydb";
        public const String DBNAME_PART = "mydb2";

	    public static DatabaseConfigServiceImpl MakeService()
		{
            IDictionary<string, ConfigurationDBRef> configs = new Dictionary<String, ConfigurationDBRef>();

            configs.Put(DBNAME_FULL, DbConfigReferenceNative);
            configs.Put(DBNAME_PART, DbConfigReferenceODBC);

            return new DatabaseConfigServiceImpl(
                configs, new SupportSchedulingServiceImpl(), null, 
                SupportEngineImportServiceFactory.Make(SupportContainer.Instance));
		}

        public static Properties DefaultProperties
        {
            get
            {
                var serverHost = Environment.GetEnvironmentVariable("ESPER_MYSQL_HOST");
                var serverUser = Environment.GetEnvironmentVariable("ESPER_MYSQL_USER");
                var serverPass = Environment.GetEnvironmentVariable("ESPER_MYSQL_PASSWORD");
                var serverDbase = Environment.GetEnvironmentVariable("ESPER_MYSQL_DBASE");

                if (serverHost == null)
                    serverHost = "nesper-mysql-integ.local";
                if (serverUser == null)
                    serverUser = "esper";
                if (serverPass == null)
                    serverPass = "3sp3rP@ssw0rd";
                if (serverDbase == null)
                    serverDbase = "test";

                var properties = new Properties();
                properties["Server"] = serverHost;
                properties["Uid"] = serverUser;
                properties["Pwd"] = serverPass;
                properties["Database"] = serverDbase;

                return properties;
            }
        }

	    public static ConfigurationDBRef CreateDefaultConfig(Properties properties = null)
	    {
	        var configDB = new ConfigurationDBRef();
	        configDB.SetDatabaseDriver(DbDriverFactoryDefault, properties);
	        return configDB;
	    }
	}
}
