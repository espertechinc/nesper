///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.client
{
    /// <summary>Configuration information for plugging in a custom single-row function. </summary>
    [Serializable]
    public class ConfigurationPlugInSingleRowFunction 
    {
        /// <summary>Ctor. </summary>
        public ConfigurationPlugInSingleRowFunction()
        {
            ValueCache = ValueCache.DISABLED;
            FilterOptimizable = FilterOptimizable.ENABLED;
        }

        /// <summary>Returns the single-row function name for use in EPL. </summary>
        /// <value>single-row function name</value>
        public string Name { get; set; }

        /// <summary>Returns the single-row function name. </summary>
        /// <value>name</value>
        public string FunctionClassName { get; set; }

        /// <summary>Returns the name of the single-row function. </summary>
        /// <value>function name</value>
        public string FunctionMethodName { get; set; }

        /// <summary>Returns the setting for the cache behavior. </summary>
        /// <value>cache behavior</value>
        public ValueCache ValueCache { get; set; }

        public FilterOptimizable FilterOptimizable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the engine re-throws exceptions
        /// thrown by the single-row function.  The default is false, therefore the
        /// engine by default does not rethrow exceptions.
        /// </summary>
        /// <value><c>true</c> if [rethrow exceptions]; otherwise, <c>false</c>.</value>
        public bool RethrowExceptions { get; set; }
    }

    /// <summary>Enum for single-row function value cache setting. </summary>
    public enum ValueCache
    {
        /// <summary>The default, the result of a single-row function is always computed anew. </summary>
        DISABLED,

        /// <summary>Causes the engine to not actually invoke the single-row function and instead return a cached precomputed value when all parameters are constants or there are no parameters. </summary>
        ENABLED,

        /// <summary>Causes the engine to follow the engine-wide policy as configured for user-defined functions. </summary>
        CONFIGURED
    }

    public enum FilterOptimizable
    {
        DISABLED,
        ENABLED
    }
}
