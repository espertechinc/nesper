///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration.compiler;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.client.hook.aggmultifunc
{
	/// <summary>
	/// Context for use with <seealso cref="com.espertech.esper.common.client.hook.aggfunc.AggregationFunctionForge" /> provides
	/// information about an aggregation function at the time of validation.
	/// <para />At validation time the event type information, parameter expressions
	/// and other statement-specific services are available.
	/// </summary>
	public class AggregationMultiFunctionValidationContext {
	    private readonly string functionName;
	    private readonly EventType[] eventTypes;
	    private readonly ExprNode[] parameterExpressions;
	    private readonly string statementName;
	    private readonly ExprValidationContext validationContext;
	    private readonly ConfigurationCompilerPlugInAggregationMultiFunction config;
	    private readonly TableMetadataColumnAggregation optionalTableColumnRead;
	    private readonly ExprNode[] allParameterExpressions;
	    private readonly ExprNode optionalFilterExpression;

	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    /// <param name="functionName">function name</param>
	    /// <param name="eventTypes">event types</param>
	    /// <param name="parameterExpressions">expressions</param>
	    /// <param name="statementName">statement name</param>
	    /// <param name="validationContext">validation context</param>
	    /// <param name="config">configuration</param>
	    /// <param name="optionalTableColumnRead">optional table column name</param>
	    /// <param name="allParameterExpressions">all parameters</param>
	    /// <param name="optionalFilterExpression">optional filter parameter</param>
	    public AggregationMultiFunctionValidationContext(string functionName, EventType[] eventTypes, ExprNode[] parameterExpressions, string statementName, ExprValidationContext validationContext, ConfigurationCompilerPlugInAggregationMultiFunction config, TableMetadataColumnAggregation optionalTableColumnRead, ExprNode[] allParameterExpressions, ExprNode optionalFilterExpression) {
	        this.functionName = functionName;
	        this.eventTypes = eventTypes;
	        this.parameterExpressions = parameterExpressions;
	        this.statementName = statementName;
	        this.validationContext = validationContext;
	        this.config = config;
	        this.optionalTableColumnRead = optionalTableColumnRead;
	        this.allParameterExpressions = allParameterExpressions;
	        this.optionalFilterExpression = optionalFilterExpression;
	    }

	    /// <summary>
	    /// Returns the aggregation function name
	    /// </summary>
	    /// <returns>aggregation function name</returns>
	    public string FunctionName {
	        get => functionName;	    }

	    /// <summary>
	    /// Returns the event types of all events in the select clause
	    /// </summary>
	    /// <returns>types</returns>
	    public EventType[] GetEventTypes() {
	        return eventTypes;
	    }

	    /// <summary>
	    /// Returns positional parameters expressions to this aggregation function.
	    /// Use {@link #getAllParameterExpressions()} for a list of all parameters including non-positional parameters.
	    /// </summary>
	    /// <returns>positional parameter expressions</returns>
	    public ExprNode[] GetParameterExpressions() {
	        return parameterExpressions;
	    }

	    /// <summary>
	    /// Returns the statement name.
	    /// </summary>
	    /// <returns>statement name</returns>
	    public string StatementName {
	        get => statementName;	    }

	    /// <summary>
	    /// Returns additional validation contextual services.
	    /// </summary>
	    /// <returns>validation context</returns>
	    public ExprValidationContext ValidationContext {
	        get => validationContext;	    }

	    /// <summary>
	    /// Returns the original configuration object for the aggregation multi-function
	    /// </summary>
	    /// <returns>config</returns>
	    public ConfigurationCompilerPlugInAggregationMultiFunction Config {
	        get => config;	    }

	    /// <summary>
	    /// Returns positional and non-positional parameters.
	    /// </summary>
	    /// <returns>all parameters</returns>
	    public ExprNode[] GetAllParameterExpressions() {
	        return allParameterExpressions;
	    }

	    /// <summary>
	    /// Returns the filter expression when provided
	    /// </summary>
	    /// <returns>filter expression</returns>
	    public ExprNode OptionalFilterExpression {
	        get => optionalFilterExpression;	    }

	    /// <summary>
	    /// Returns table column information when used with tables
	    /// </summary>
	    /// <returns>table column</returns>
	    public TableMetadataColumnAggregation OptionalTableColumnRead {
	        get => optionalTableColumnRead;	    }

	    /// <summary>
	    /// Gets the named parameters as a list
	    /// </summary>
	    /// <returns>named params</returns>
	    public LinkedHashMap<string, IList<ExprNode>> GetNamedParameters() {
	        LinkedHashMap<string, IList<ExprNode>> named = new LinkedHashMap<string,  IList<ExprNode>>();
	        foreach (ExprNode node in allParameterExpressions) {
	            if (node is ExprNamedParameterNode) {
	                ExprNamedParameterNode namedNode = (ExprNamedParameterNode) node;
	                named.Put(namedNode.ParameterName, Arrays.AsList(namedNode.ChildNodes));
	            }
	        }
	        return named;
	    }
	}
} // end of namespace