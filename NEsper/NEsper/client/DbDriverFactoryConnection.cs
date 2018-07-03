///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;

using com.espertech.esper.compat;
using com.espertech.esper.compat.container;
using com.espertech.esper.epl.db;
using com.espertech.esper.util;

namespace com.espertech.esper.client
{
    /// <summary>
    /// Connection factory settings for using a DbDriverFactory.
    /// </summary>

    [Serializable]
    public class DbDriverFactoryConnection : ConnectionFactoryDesc
    {
        /// <summary>
        /// Database driver.
        /// </summary>
        private readonly DbDriver _driver;

        /// <summary>
        /// Gets the database driver.
        /// </summary>
        /// <value>The database driver.</value>
        public virtual DbDriver Driver => _driver;

        /// <summary>
        /// Where do drivers in the default search path reside
        /// </summary>

        public const string DriverNamespace = "com.espertech.esper.epl.db.drivers";

        /// <summary>
        /// Resolves the driver type from the name provided.  If the driver can not be
        /// resolved, the method throws an exception to indicate that one could not
        /// be found.  The method first looks for a class that matches the name of
        /// the driver.  If one can not be found, it checks in the com.espertech.esper.epl.drivers
        /// namespace to see if one can be found.  Lastly, if checks in the
        /// com.espertech.espers.eql.drivers namespace with a variation of the given driver name.
        /// </summary>
        /// <param name="driverName">Name of the driver.</param>
        /// <returns></returns>
        public static Type ResolveDriverTypeFromName(String driverName)
        {
            Type driverType;

            // Check for the type with no modifications
            if ((driverType = TypeHelper.ResolveType(driverName, false)) != null)
            {
                return driverType;
            }

            // Check for the type in the driverNamespace
            String specificName = String.Format("{0}.{1}", DriverNamespace, driverName);
            if ((driverType = TypeHelper.ResolveType(specificName, false)) != null)
            {
                return driverType;
            }

            // Check for the type in the driverNamespace, but modified to include
            // a prefix to the name.
            String pseudoName = String.Format("{0}.DbDriver{1}", DriverNamespace, driverName);
            if ((driverType = TypeHelper.ResolveType(pseudoName, false)) != null)
            {
                return driverType;
            }

            // Driver was not found, throw an exception
            throw new EPException("Unable to resolve type for driver '" + driverName + "'");
        }

        /// <summary>
        /// Resolves the driver from the name.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="driverName">Name of the driver.</param>
        /// <returns></returns>
        public static DbDriver ResolveDriverFromName(IContainer container, String driverName)
        {
            return ResolveDriverFromType(container, 
                ResolveDriverTypeFromName(driverName));
        }

        /// <summary>
        /// Resolves the driver from the type.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="driverType">Type of the driver.</param>
        public static DbDriver ResolveDriverFromType(IContainer container, Type driverType)
        {
            if (typeof(DbDriver).IsAssignableFrom(driverType)) {
                return container.Resolve<DbDriver>(driverType.FullName);
                //return Activator.CreateInstance(driverType) as DbDriver;
            }

            throw new EPException("Unable to create driver because it was not assignable from " +
                                  typeof(DbDriver).FullName);
        }

#if false
        /// <summary>
        /// Initializes a new instance of the <see cref="DbDriverFactoryConnection"/> class.
        /// </summary>
        /// <param name="driverType">Type of the driver.</param>
        /// <param name="properties">The properties.</param>
        public DbDriverFactoryConnection(IContainer container, Type driverType, Properties properties)
        {
            _driver = ResolveDriverFromType(container, driverType);
            _driver.Properties = properties;
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="driverName">Name of the driver.</param>
        /// <param name="properties">Properties that should be applied to the connection.</param>

        public DbDriverFactoryConnection(IContainer container, String driverName, Properties properties)
        {
            _driver = ResolveDriverFromName(container, driverName);
            _driver.Properties = properties;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DbDriverFactoryConnection"/> class.
        /// </summary>
        /// <param name="dbSpecification">The db specification.</param>
        public DbDriverFactoryConnection(IContainer container, DbDriverConfiguration dbSpecification)
        {
            _driver = ResolveDriverFromName(container, dbSpecification.DriverName);
            _driver.Properties = dbSpecification.Properties;
        }
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="DbDriverFactoryConnection"/> class.
        /// </summary>
        /// <param name="driver">The driver.</param>
        public DbDriverFactoryConnection(DbDriver driver)
        {
            _driver = driver;
        }
    }
}
