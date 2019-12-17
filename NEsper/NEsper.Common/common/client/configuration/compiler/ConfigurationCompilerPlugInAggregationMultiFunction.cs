///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.common.client.configuration.compiler
{
    /// <summary>
    ///     Configuration information for plugging in a custom aggregation multi-function.
    /// </summary>
    [Serializable]
    public class ConfigurationCompilerPlugInAggregationMultiFunction
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        public ConfigurationCompilerPlugInAggregationMultiFunction()
        {
        }

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="functionNames">the aggregation function names</param>
        /// <param name="multiFunctionForgeClassName">the factory class name</param>
        public ConfigurationCompilerPlugInAggregationMultiFunction(
            string[] functionNames,
            string multiFunctionForgeClassName)
        {
            this.FunctionNames = functionNames;
            this.MultiFunctionForgeClassName = multiFunctionForgeClassName;
        }

        public ConfigurationCompilerPlugInAggregationMultiFunction(
            string[] functionNames,
            Type multiFunctionForgeClass)
        {
            this.FunctionNames = functionNames;
            this.MultiFunctionForgeClassName = multiFunctionForgeClass.FullName;
        }

        /// <summary>
        ///     Returns aggregation function names.
        /// </summary>
        /// <value>names</value>
        public string[] FunctionNames { get; set; }

        /// <summary>
        ///     Returns the factory class name.
        /// </summary>
        /// <value>class name</value>
        public string MultiFunctionForgeClassName { get; set; }

        /// <summary>
        ///     Returns a map of optional configuration properties, or null if none provided.
        /// </summary>
        /// <value>additional optional properties</value>
        public IDictionary<string, object> AdditionalConfiguredProperties { get; set; }
    }
} // end of namespace