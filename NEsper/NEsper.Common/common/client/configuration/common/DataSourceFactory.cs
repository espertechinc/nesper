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
    ///     Connection factory settings for using a Apache DBCP or other provider DataSource factory.
    /// </summary>
    [Serializable]
    public class DataSourceFactory : ConnectionFactoryDesc
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="properties">to pass to the data source factory</param>
        /// <param name="factoryClassname">the class name of the data source factory</param>
        public DataSourceFactory(
            Properties properties,
            string factoryClassname)
        {
            Properties = properties;
            FactoryClassname = factoryClassname;
        }

        /// <summary>
        ///     Returns the properties to pass to the static createDataSource method provided.
        /// </summary>
        /// <returns>properties to pass to createDataSource</returns>
        public Properties Properties { get; }

        /// <summary>
        ///     Returns the class name of the data source factory.
        /// </summary>
        /// <returns>fully qualified class name</returns>
        public string FactoryClassname { get; }

        /// <summary>
        ///     Adds a property.
        /// </summary>
        /// <param name="name">key</param>
        /// <param name="value">value of property</param>
        public void AddProperty(
            string name,
            string value)
        {
            Properties.Put(name, value);
        }
    }
}