///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.client.configuration.common
{
    /// <summary>
    ///     Connection factory settings for using a driver.
    /// </summary>
    public class DriverConnectionFactoryDesc : ConnectionFactoryDesc
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="driverName">is the driver name</param>
        /// <param name="driverProperties">is driver properties</param>
        public DriverConnectionFactoryDesc(
            string driverName,
            Properties driverProperties)
        {
            DriverName = driverName;
            DriverProperties = driverProperties;
        }

        /// <summary>
        ///     Returns the driver manager class name.
        /// </summary>
        /// <returns>class name of driver manager</returns>
        public string DriverName { get; }

        /// <summary>
        ///     Returns the properties to use for obtaining a connection via driver manager.
        /// </summary>
        public Properties DriverProperties { get; }

        /// <summary>
        /// Merges the specified properties to merge.
        /// </summary>
        /// <param name="propertiesToMerge">The properties to merge.</param>
        /// <returns></returns>
        public DriverConnectionFactoryDesc Merge(Properties propertiesToMerge)
        {
            var newProperties = new Properties();
            newProperties.PutAll(DriverProperties);
            newProperties.PutAll(propertiesToMerge);
            return new DriverConnectionFactoryDesc(DriverName, newProperties);
        }
    }
}