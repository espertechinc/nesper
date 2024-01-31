///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.client.configuration.compiler
{
    /// <summary>
    ///     Configuration information for plugging in a custom single-row function.
    /// </summary>
    public class ConfigurationCompilerPlugInSingleRowFunction
    {
        /// <summary>
        ///     Controls whether a single-row function is eligible for optimization if it occurs in a filter expression.
        /// </summary>
        public enum FilterOptimizableEnum
        {
            /// <summary>
            ///     The runtime does not consider the single-row function for optimizing evaluation: The function gets evaluated for
            ///     each event possibly multiple times.
            /// </summary>
            DISABLED,

            /// <summary>
            ///     The runtime considers the single-row function for optimizing evaluation: The function gets evaluated only once per
            ///     event.
            /// </summary>
            ENABLED
        }

        /// <summary>
        ///     Enum for single-row function value cache setting.
        /// </summary>
        public enum ValueCacheEnum
        {
            /// <summary>
            ///     The default, the result of a single-row function is always computed anew.
            /// </summary>
            DISABLED,

            /// <summary>
            ///     Causes the runtime to not actually invoke the single-row function and instead return a cached precomputed value
            ///     when all parameters are constants or there are no parameters.
            /// </summary>
            ENABLED,

            /// <summary>
            ///     Causes the runtime to follow the runtime-wide policy as configured for user-defined functions.
            /// </summary>
            CONFIGURED
        }

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="name">UDF name</param>
        /// <param name="functionClassName">class name</param>
        /// <param name="functionMethodName">method name</param>
        /// <param name="valueCache">value cache</param>
        /// <param name="filterOptimizable">optimizable setting</param>
        /// <param name="rethrowExceptions">rethrow setting</param>
        /// <param name="eventTypeName">optional event type name</param>
        public ConfigurationCompilerPlugInSingleRowFunction(
            string name,
            string functionClassName,
            string functionMethodName,
            ValueCacheEnum valueCache,
            FilterOptimizableEnum filterOptimizable,
            bool rethrowExceptions,
            string eventTypeName)
        {
            Name = name;
            FunctionClassName = functionClassName;
            FunctionMethodName = functionMethodName;
            ValueCache = valueCache;
            FilterOptimizable = filterOptimizable;
            RethrowExceptions = rethrowExceptions;
            EventTypeName = eventTypeName;
        }

        /// <summary>
        ///     Ctor.
        /// </summary>
        public ConfigurationCompilerPlugInSingleRowFunction()
        {
        }

        /// <summary>
        ///     Returns the single-row function name for use in EPL.
        /// </summary>
        /// <value>single-row function name</value>
        public string Name { get; set; }

        /// <summary>
        ///     Returns the single-row function name.
        /// </summary>
        /// <value>name</value>
        public string FunctionClassName { get; set; }

        /// <summary>
        ///     Returns the name of the single-row function.
        /// </summary>
        /// <value>function name</value>
        public string FunctionMethodName { get; set; }

        /// <summary>
        ///     Returns the setting for the cache behavior.
        /// </summary>
        /// <value>cache behavior</value>
        public ValueCacheEnum ValueCache { get; set; } = ValueCacheEnum.DISABLED;

        /// <summary>
        ///     Returns filter optimization settings.
        /// </summary>
        /// <value>filter optimization settings</value>
        public FilterOptimizableEnum FilterOptimizable { get; set; } = FilterOptimizableEnum.ENABLED;

        /// <summary>
        ///     Returns indicator whether the runtime re-throws exceptions
        ///     thrown by the single-row function. The default is false
        ///     therefore the runtime by default does not rethrow exceptions.
        /// </summary>
        /// <value>indicator</value>
        public bool RethrowExceptions { get; set; }

        /// <summary>
        ///     Returns the event type name for functions that return <seealso cref="EventBean" /> instances.
        /// </summary>
        /// <value>event type name</value>
        public string EventTypeName { get; set; }
    }
} // end of namespace