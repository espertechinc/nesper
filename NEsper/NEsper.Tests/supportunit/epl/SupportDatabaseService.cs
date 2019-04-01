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
using System.IO;
using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.support;
using com.espertech.esper.epl.db;
using com.espertech.esper.supportunit.util;

namespace com.espertech.esper.supportunit.epl
{
    using Configuration = esper.client.Configuration;
    
    public class SupportDatabaseService
	{
	    private const string ESPER_LOCAL_CONFIG_FILE = "NEsperConfig.xml";

	    protected internal const String ESPER_TEST_CONFIG = "regression/esper.test.readconfig.cfg.xml";

        public static readonly ConfigurationDBRef DbConfigReferenceNative;
        public static readonly ConfigurationDBRef DbConfigReferenceODBC;

	    public static readonly DbDriverFactoryConnection DbDriverFactoryNative;
        public static readonly DbDriverFactoryConnection DbDriverFactoryODBC;

        static SupportDatabaseService()
        {
            var configurationFile = new FileInfo(ESPER_LOCAL_CONFIG_FILE);
            var configuration = new Configuration(SupportContainer.Instance);
            configuration.Configure(configurationFile);

            var dbTable = configuration.DatabaseReferences;

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
	    public static DbDriver DriverNative
	    {
            get { return DbDriverFactoryNative.Driver; }
	    }

        /// <summary>
        /// Gets the second database driver.
        /// </summary>
        public static DbDriver DriverODBC
        {
            get { return DbDriverFactoryODBC.Driver; }
        }

        public const String DBNAME_FULL = "mydb";
        public const String DBNAME_PART = "mydb2";

	    public static DatabaseConfigServiceImpl MakeService()
		{
            IDictionary<string, ConfigurationDBRef> configs = new Dictionary<String, ConfigurationDBRef>();

            configs.Put(DBNAME_FULL, DbConfigReferenceNative);
            configs.Put(DBNAME_PART, DbConfigReferenceODBC);

            return new DatabaseConfigServiceImpl(configs, 
                new SupportSchedulingServiceImpl(), null, 
                SupportEngineImportServiceFactory.Make(SupportContainer.Instance));
		}

        public static Properties DefaultProperties
        {
            get
            {
                var serverHost = Environment.GetEnvironmentVariable("ESPER_MYSQL_HOST");
                var serverUser = Environment.GetEnvironmentVariable("ESPER_MYSQL_USER");
                var serverPass = Environment.GetEnvironmentVariable("ESPER_MYSQL_PASSWORD");

                if (serverHost == null)
                    serverHost = "nesper-mysql-integ.local";
                if (serverUser == null)
                    serverUser = "esper";
                if (serverPass == null)
                    serverPass = "3sp3rP@ssw0rd";

                var properties = new Properties();
                properties["Server"] = serverHost;
                properties["Uid"] = serverUser;
                properties["Pwd"] = serverPass;

                return properties;
            }
        }
	}
}
