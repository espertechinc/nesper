///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.client
{
    /// <summary>
    /// Configuration information for plugging in a custom aggregation multi-function.
    /// </summary>
    [Serializable]
    public class ConfigurationPlugInAggregationMultiFunction 
    {
        /// <summary>Ctor. </summary>
        public ConfigurationPlugInAggregationMultiFunction()
        {
        }
    
        /// <summary>Ctor. </summary>
        /// <param name="functionNames">the aggregation function names</param>
        /// <param name="multiFunctionFactoryClassName">the factory class name</param>
        public ConfigurationPlugInAggregationMultiFunction(String[] functionNames, String multiFunctionFactoryClassName)
        {
            FunctionNames = functionNames;
            MultiFunctionFactoryClassName = multiFunctionFactoryClassName;
        }

        public ConfigurationPlugInAggregationMultiFunction(String[] functionNames, Type multiFunctionFactoryClass)
            : this(functionNames, multiFunctionFactoryClass.AssemblyQualifiedName)
        {
        }

        /// <summary>Returns aggregation function names. </summary>
        /// <value>names</value>
        public string[] FunctionNames { get; set; }

        /// <summary>Returns the factory class name. </summary>
        /// <value>class name</value>
        public string MultiFunctionFactoryClassName { get; set; }

        /// <summary>Returns a map of optional configuration properties, or null if none provided. </summary>
        /// <value>additional optional properties</value>
        public IDictionary<string, object> AdditionalConfiguredProperties { get; set; }
    }
}
