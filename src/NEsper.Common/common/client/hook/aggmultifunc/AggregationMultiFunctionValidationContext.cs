///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.configuration.compiler;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.client.hook.aggmultifunc
{
    /// <summary>
    ///     Context for use with <seealso cref="com.espertech.esper.common.client.hook.aggfunc.AggregationFunctionForge" />
    ///     provides
    ///     information about an aggregation function at the time of validation.
    ///     <para />
    ///     At validation time the event type information, parameter expressions
    ///     and other statement-specific services are available.
    /// </summary>
    public class AggregationMultiFunctionValidationContext
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="functionName">function name</param>
        /// <param name="eventTypes">event types</param>
        /// <param name="parameterExpressions">expressions</param>
        /// <param name="statementName">statement name</param>
        /// <param name="validationContext">validation context</param>
        /// <param name="config">configuration</param>
        /// <param name="allParameterExpressions">all parameters</param>
        /// <param name="optionalFilterExpression">optional filter parameter</param>
        public AggregationMultiFunctionValidationContext(
            string functionName,
            EventType[] eventTypes,
            ExprNode[] parameterExpressions,
            string statementName,
            ExprValidationContext validationContext,
            ConfigurationCompilerPlugInAggregationMultiFunction config,
            ExprNode[] allParameterExpressions,
            ExprNode optionalFilterExpression)
        {
            FunctionName = functionName;
            EventTypes = eventTypes;
            ParameterExpressions = parameterExpressions;
            StatementName = statementName;
            ValidationContext = validationContext;
            Config = config;
            AllParameterExpressions = allParameterExpressions;
            OptionalFilterExpression = optionalFilterExpression;
        }

        /// <summary>
        ///     Returns the aggregation function name
        /// </summary>
        /// <returns>aggregation function name</returns>
        public string FunctionName { get; }

        /// <summary>
        ///     Returns the event types of all events in the select clause
        /// </summary>
        /// <value>types</value>
        public EventType[] EventTypes { get; }

        /// <summary>
        ///     Returns positional parameters expressions to this aggregation function.
        ///     Use <seealso cref="AllParameterExpressions" /> for a list of all parameters including non-positional parameters.
        /// </summary>
        /// <value>positional parameter expressions</value>
        public ExprNode[] ParameterExpressions { get; }

        /// <summary>
        ///     Returns the statement name.
        /// </summary>
        /// <returns>statement name</returns>
        public string StatementName { get; }

        /// <summary>
        ///     Returns additional validation contextual services.
        /// </summary>
        /// <returns>validation context</returns>
        public ExprValidationContext ValidationContext { get; }

        /// <summary>
        ///     Returns the original configuration object for the aggregation multi-function
        /// </summary>
        /// <returns>config</returns>
        public ConfigurationCompilerPlugInAggregationMultiFunction Config { get; }

        /// <summary>
        ///     Returns positional and non-positional parameters.
        /// </summary>
        /// <value>all parameters</value>
        public ExprNode[] AllParameterExpressions { get; }

        /// <summary>
        ///     Returns the filter expression when provided
        /// </summary>
        /// <returns>filter expression</returns>
        public ExprNode OptionalFilterExpression { get; }

        /// <summary>
        ///     Gets the named parameters as a list
        /// </summary>
        /// <value>named params</value>
        public LinkedHashMap<string, IList<ExprNode>> NamedParameters {
            get {
                var named = new LinkedHashMap<string, IList<ExprNode>>();
                foreach (var node in AllParameterExpressions) {
                    if (node is ExprNamedParameterNode namedNode) {
                        named.Put(namedNode.ParameterName, Arrays.AsList(namedNode.ChildNodes));
                    }
                }

                return named;
            }
        }
    }
} // end of namespace