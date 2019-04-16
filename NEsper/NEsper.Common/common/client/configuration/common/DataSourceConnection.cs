///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.client.configuration.common
{
    /// <summary>
    ///     Connection factory settings for using a DataSource.
    /// </summary>
    [Serializable]
    public class DataSourceConnection : ConnectionFactoryDesc
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="contextLookupName">is the object name to look up</param>
        /// <param name="envProperties">are the context properties to use constructing InitialContext</param>
        public DataSourceConnection(
            string contextLookupName,
            Properties envProperties)
        {
            ContextLookupName = contextLookupName;
            EnvProperties = envProperties;
        }

        /// <summary>
        ///     Returns the object name to look up in context.
        /// </summary>
        /// <returns>object name</returns>
        public string ContextLookupName { get; }

        /// <summary>
        ///     Returns the environment properties to use to establish the initial context.
        /// </summary>
        /// <returns>environment properties to construct the initial context</returns>
        public Properties EnvProperties { get; }
    }
}