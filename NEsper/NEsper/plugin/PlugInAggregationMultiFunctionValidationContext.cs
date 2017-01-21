///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.table.mgmt;

namespace com.espertech.esper.plugin
{
    /// <summary>
    /// Context for use with <seealso cref="PlugInAggregationMultiFunctionFactory"/> provides 
    /// information about an aggregation function at the time of validation.
    /// <para/>
    /// At validation time the event type information, parameter expressions and other 
    /// statement-specific services are available.
    /// </summary>
    public class PlugInAggregationMultiFunctionValidationContext
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="functionName">the aggregation function name</param>
        /// <param name="eventTypes">the event types of all events in the select clause</param>
        /// <param name="parameterExpressions">the parameter expressions</param>
        /// <param name="engineURI">the engine URI</param>
        /// <param name="statementName">the statement name</param>
        /// <param name="validationContext">additional validation contextual services</param>
        /// <param name="config">the original configuration object for the aggregation multi-function</param>
        /// <param name="optionalTableColumnAccessed">The optional table column accessed.</param>
        /// <param name="allParameterExpressions">All parameter expressions.</param>
        public PlugInAggregationMultiFunctionValidationContext(
            string functionName,
            EventType[] eventTypes,
            ExprNode[] parameterExpressions,
            string engineURI,
            string statementName,
            ExprValidationContext validationContext,
            ConfigurationPlugInAggregationMultiFunction config,
            TableMetadataColumnAggregation optionalTableColumnAccessed,
            ExprNode[] allParameterExpressions)
        {
            FunctionName = functionName;
            EventTypes = eventTypes;
            ParameterExpressions = parameterExpressions;
            EngineURI = engineURI;
            StatementName = statementName;
            ValidationContext = validationContext;
            Config = config;
            OptionalTableColumnAccessed = optionalTableColumnAccessed;
            AllParameterExpressions = allParameterExpressions;
        }

        /// <summary>Returns the aggregation function name </summary>
        /// <value>aggregation function name</value>
        public string FunctionName { get; private set; }

        /// <summary>Returns the event types of all events in the select clause </summary>
        /// <value>types</value>
        public EventType[] EventTypes { get; private set; }

        /// <summary>
        /// Returns positional parameters expressions to this aggregation function.
        /// Use <seealso cref="AllParameterExpressions" /> for a list of all parameters including non-positional parameters.
        /// </summary>
        public ExprNode[] ParameterExpressions { get; private set; }

        /// <summary>Returns the engine URI. </summary>
        /// <value>engine URI.</value>
        public string EngineURI { get; private set; }

        /// <summary>Returns the statement name. </summary>
        /// <value>statement name</value>
        public string StatementName { get; private set; }

        /// <summary>Returns additional validation contextual services. </summary>
        /// <value>validation context</value>
        public ExprValidationContext ValidationContext { get; private set; }

        /// <summary>Returns the original configuration object for the aggregation multi-function </summary>
        /// <value>config</value>
        public ConfigurationPlugInAggregationMultiFunction Config { get; private set; }

        /// <summary>
        /// Returns the optional table column.
        /// </summary>
        /// <value>
        /// The optional table column.
        /// </value>
        public TableMetadataColumnAggregation OptionalTableColumnAccessed { get; private set; }

        /// <summary>
        /// Returns positional and non-positional parameters.
        /// </summary>
        public ExprNode[] AllParameterExpressions { get; private set; }
    }
}
