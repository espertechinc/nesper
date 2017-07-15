///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.table.mgmt;

namespace com.espertech.esper.plugin
{
    /// <summary>
    /// Context for use with <seealso cref="PlugInAggregationMultiFunctionFactory" /> provides
    /// information about an aggregation function at the time of validation.
    /// <para>
    /// At validation time the event type information, parameter expressions
    /// and other statement-specific services are available.
    /// </para>
    /// </summary>
    public class PlugInAggregationMultiFunctionValidationContext {
        private readonly string functionName;
        private readonly EventType[] eventTypes;
        private readonly ExprNode[] parameterExpressions;
        private readonly string engineURI;
        private readonly string statementName;
        private readonly ExprValidationContext validationContext;
        private readonly ConfigurationPlugInAggregationMultiFunction config;
        private readonly TableMetadataColumnAggregation optionalTableColumnAccessed;
        private readonly ExprNode[] allParameterExpressions;
    
        public PlugInAggregationMultiFunctionValidationContext(string functionName, EventType[] eventTypes, ExprNode[] parameterExpressions, string engineURI, string statementName, ExprValidationContext validationContext, ConfigurationPlugInAggregationMultiFunction config, TableMetadataColumnAggregation optionalTableColumnAccessed, ExprNode[] allParameterExpressions) {
            this.functionName = functionName;
            this.eventTypes = eventTypes;
            this.parameterExpressions = parameterExpressions;
            this.engineURI = engineURI;
            this.statementName = statementName;
            this.validationContext = validationContext;
            this.config = config;
            this.optionalTableColumnAccessed = optionalTableColumnAccessed;
            this.allParameterExpressions = allParameterExpressions;
        }
    
        /// <summary>
        /// Returns the aggregation function name
        /// </summary>
        /// <returns>aggregation function name</returns>
        public string GetFunctionName() {
            return functionName;
        }
    
        /// <summary>
        /// Returns the event types of all events in the select clause
        /// </summary>
        /// <returns>types</returns>
        public EventType[] GetEventTypes() {
            return eventTypes;
        }
    
        /// <summary>
        /// Returns positional parameters expressions to this aggregation function.
        /// Use {@link #GetAllParameterExpressions()} for a list of all parameters including non-positional parameters.
        /// </summary>
        /// <returns>positional parameter expressions</returns>
        public ExprNode[] GetParameterExpressions() {
            return parameterExpressions;
        }
    
        /// <summary>
        /// Returns the engine URI.
        /// </summary>
        /// <returns>engine URI.</returns>
        public string GetEngineURI() {
            return engineURI;
        }
    
        /// <summary>
        /// Returns the statement name.
        /// </summary>
        /// <returns>statement name</returns>
        public string GetStatementName() {
            return statementName;
        }
    
        /// <summary>
        /// Returns additional validation contextual services.
        /// </summary>
        /// <returns>validation context</returns>
        public ExprValidationContext GetValidationContext() {
            return validationContext;
        }
    
        /// <summary>
        /// Returns the original configuration object for the aggregation multi-function
        /// </summary>
        /// <returns>config</returns>
        public ConfigurationPlugInAggregationMultiFunction GetConfig() {
            return config;
        }
    
        public TableMetadataColumnAggregation GetOptionalTableColumnAccessed() {
            return optionalTableColumnAccessed;
        }
    
        /// <summary>
        /// Returns positional and non-positional parameters.
        /// </summary>
        /// <returns>all parameters</returns>
        public ExprNode[] GetAllParameterExpressions() {
            return allParameterExpressions;
        }
    }
} // end of namespace
