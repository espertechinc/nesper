///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.configuration.compiler;

namespace com.espertech.esper.common.client.hook.aggmultifunc
{
    /// <summary>
    /// Context for use with <seealso cref="AggregationMultiFunctionForge" /> provides
    /// information about an aggregation function at the time of declaration.
    /// <para />Declaration means when the aggregation function is discovered at the time
    /// of parsing an EPL statement. Or when using statement object model
    /// then at the time of mapping the object model to the
    /// internal statement representation.
    /// </summary>
    public class AggregationMultiFunctionDeclarationContext
    {
        private readonly string _functionName;
        private readonly bool _distinct;
        private ConfigurationCompilerPlugInAggregationMultiFunction _configuration;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="functionName">provides the aggregation multi-function name</param>
        /// <param name="distinct">flag whether the "distinct" keyword was provided.</param>
        /// <param name="configuration">the configuration provided when the aggregation multi-functions where registered</param>
        public AggregationMultiFunctionDeclarationContext(
            string functionName,
            bool distinct,
            ConfigurationCompilerPlugInAggregationMultiFunction configuration)
        {
            this._functionName = functionName;
            this._distinct = distinct;
            this._configuration = configuration;
        }

        /// <summary>
        /// Returns a flag whether the "distinct" keyword was provided.
        /// </summary>
        /// <returns>distinct flag</returns>
        public bool IsDistinct()
        {
            return _distinct;
        }

        /// <summary>
        /// Returns the aggregation function name.
        /// </summary>
        /// <returns>function name</returns>
        public string FunctionName {
            get => _functionName;
        }

        /// <summary>
        /// Returns the configuration provided when the aggregation multi-functions where registered.
        /// </summary>
        /// <returns>configuration</returns>
        public ConfigurationCompilerPlugInAggregationMultiFunction Configuration {
            get => _configuration;
        }
    }
} // end of namespace