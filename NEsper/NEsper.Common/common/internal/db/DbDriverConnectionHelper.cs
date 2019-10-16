///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.container;

namespace com.espertech.esper.common.@internal.db
{
    /// <summary>
    /// Connection factory settings for using a DbDriverFactory.
    /// </summary>
    public static class DbDriverConnectionHelper
    {
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
        public static Type ResolveDriverTypeFromName(string driverName)
        {
            Type driverType;

            // Check for the type with no modifications
            if ((driverType = TypeHelper.ResolveType(driverName, false)) != null) {
                return driverType;
            }

            // Check for the type in the driverNamespace
            string specificName = $"{DriverNamespace}.{driverName}";
            if ((driverType = TypeHelper.ResolveType(specificName, false)) != null) {
                return driverType;
            }

            // Check for the type in the driverNamespace, but modified to include
            // a prefix to the name.
            string pseudoName = $"{DriverNamespace}.DbDriver{driverName}";
            if ((driverType = TypeHelper.ResolveType(pseudoName, false)) != null) {
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
        public static DbDriver ResolveDriverFromName(
            IContainer container,
            string driverName)
        {
            return ResolveDriverFromType(
                container,
                ResolveDriverTypeFromName(driverName));
        }

        /// <summary>
        /// Resolves the driver from the type.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="driverType">Type of the driver.</param>
        public static DbDriver ResolveDriverFromType(
            IContainer container,
            Type driverType)
        {
            if (typeof(DbDriver).IsAssignableFrom(driverType)) {
                if (container.Has(driverType.FullName)) {
                    return container.Resolve<DbDriver>(driverType.FullName);
                }

                return Activator.CreateInstance(driverType) as DbDriver;
            }

            throw new EPException(
                "Unable to create driver because it was not assignable from " +
                typeof(DbDriver).FullName);
        }

        public static DbDriver ResolveDriver(
            IContainer container,
            DriverConnectionFactoryDesc driverConnectionFactoryDesc)
        {
            var driver = ResolveDriverFromName(container, driverConnectionFactoryDesc.DriverName);
            driver.Properties = driverConnectionFactoryDesc.DriverProperties;
            return driver;
        }

#if false
        /// <summary>
        /// Initializes a new instance of the <see cref="DriverConnectionFactoryDescDeprecated"/> class.
        /// </summary>
        /// <param name="container">Resource container.</param>
        /// <param name="driverType">Type of the driver.</param>
        /// <param name="properties">The properties.</param>
        public DriverConnectionFactoryDescDeprecated(
            IContainer container,
            Type driverType,
            Properties properties)
        {
            _driver = ResolveDriverFromType(container, driverType);
            _driver.Properties = properties;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DriverConnectionFactoryDescDeprecated"/> class.
        /// </summary>
        /// <param name="container">Resource container.</param>
        /// <param name="driverName">Name of the driver.</param>
        /// <param name="properties">Properties that should be applied to the connection.</param>
        public DriverConnectionFactoryDescDeprecated(
            IContainer container,
            string driverName,
            Properties properties)
        {
            _driver = ResolveDriverFromName(container, driverName);
            _driver.Properties = properties;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DriverConnectionFactoryDescDeprecated"/> class.
        /// </summary>
        /// <param name="container">Resource container.</param>
        /// <param name="specification">The db specification.</param>
        public DriverConnectionFactoryDescDeprecated(
            IContainer container,
            DriverConfiguration specification)
        {
            _driver = ResolveDriverFromName(container, specification.DriverName);
            _driver.Properties = specification.Properties;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DriverConnectionFactoryDescDeprecated"/> class.
        /// </summary>
        /// <param name="driver">The driver.</param>
        public DriverConnectionFactoryDescDeprecated(DbDriver driver)
        {
            _driver = driver;
        }
#endif
    }
}