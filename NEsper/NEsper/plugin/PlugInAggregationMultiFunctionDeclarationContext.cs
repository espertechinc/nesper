///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;

namespace com.espertech.esper.plugin
{
    /// <summary>
    /// Context for use with <seealso cref="PlugInAggregationMultiFunctionFactory"/> provides 
    /// information about an aggregation function at the time of declaration. 
    /// <para/>
    /// Declaration means when the aggregation function is discovered at the time of parsing an 
    /// EPL statement. Or when using statement object model then at the time of mapping the object 
    /// model to the internal statement representation.
    /// </summary>
    public class PlugInAggregationMultiFunctionDeclarationContext
    {
        /// <summary>Ctor. </summary>
        /// <param name="functionName">provides the aggregation multi-function name</param>
        /// <param name="distinct">flag whether the "distinct" keyword was provided.</param>
        /// <param name="engineURI">the engine URI</param>
        /// <param name="configuration">the configuration provided when the aggregation multi-functions where registered</param>
        public PlugInAggregationMultiFunctionDeclarationContext(String functionName, bool distinct, String engineURI, ConfigurationPlugInAggregationMultiFunction configuration) {
            FunctionName = functionName;
            IsDistinct = distinct;
            EngineURI = engineURI;
            Configuration = configuration;
        }

        /// <summary>Returns a flag whether the "distinct" keyword was provided. </summary>
        /// <value>distinct flag</value>
        public bool IsDistinct { get; private set; }

        /// <summary>Returns the engine uri. </summary>
        /// <value>engine uri</value>
        public string EngineURI { get; private set; }

        /// <summary>Returns the aggregation function name. </summary>
        /// <value>function name</value>
        public string FunctionName { get; private set; }

        /// <summary>Returns the configuration provided when the aggregation multi-functions where registered. </summary>
        /// <value>configuration</value>
        public ConfigurationPlugInAggregationMultiFunction Configuration { get; private set; }
    }
}
