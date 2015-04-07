///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Configuration;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.db;
using com.espertech.esper.support.schedule;

namespace com.espertech.esper.support.epl
{
    using Configuration = esper.client.Configuration;
    
    public class SupportDatabaseService
	{
        protected internal const String ESPER_TEST_CONFIG = "regression/esper.test.readconfig.cfg.xml";

	    public static readonly ConfigurationDBRef DbConfigReferenceNative;
        public static readonly ConfigurationDBRef DbConfigReferenceODBC;

	    public static readonly DbDriverFactoryConnection DbDriverFactoryNative;
        public static readonly DbDriverFactoryConnection DbDriverFactoryODBC;

        static SupportDatabaseService()
        {
            var configuration = ConfigurationManager.GetSection("esper-configuration") as Configuration;

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

			return new DatabaseConfigServiceImpl( configs, new SupportSchedulingServiceImpl(), null );
		}
	}
}
