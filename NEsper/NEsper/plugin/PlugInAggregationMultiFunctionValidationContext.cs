///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
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
    public class PlugInAggregationMultiFunctionValidationContext
    {
        private readonly string _functionName;
        private readonly EventType[] _eventTypes;
        private readonly ExprNode[] _parameterExpressions;
        private readonly string _engineURI;
        private readonly string _statementName;
        private readonly ExprValidationContext _validationContext;
        private readonly ConfigurationPlugInAggregationMultiFunction _config;
        private readonly TableMetadataColumnAggregation _optionalTableColumnAccessed;
        private readonly IList<ExprNode> _allParameterExpressions;

        public PlugInAggregationMultiFunctionValidationContext(
            string functionName,
            EventType[] eventTypes,
            ExprNode[] parameterExpressions,
            string engineURI,
            string statementName,
            ExprValidationContext validationContext,
            ConfigurationPlugInAggregationMultiFunction config,
            TableMetadataColumnAggregation optionalTableColumnAccessed,
            IList<ExprNode> allParameterExpressions)
        {
            _functionName = functionName;
            _eventTypes = eventTypes;
            _parameterExpressions = parameterExpressions;
            _engineURI = engineURI;
            _statementName = statementName;
            _validationContext = validationContext;
            _config = config;
            _optionalTableColumnAccessed = optionalTableColumnAccessed;
            _allParameterExpressions = allParameterExpressions;
        }

        /// <summary>
        /// Returns the aggregation function name
        /// </summary>
        /// <value>aggregation function name</value>
        public string FunctionName => _functionName;

        /// <summary>
        /// Returns the event types of all events in the select clause
        /// </summary>
        /// <value>types</value>
        public EventType[] EventTypes => _eventTypes;

        /// <summary>
        /// Returns positional parameters expressions to this aggregation function.
        /// Use {@link #GetAllParameterExpressions()} for a list of all parameters including non-positional parameters.
        /// </summary>
        /// <value>positional parameter expressions</value>
        public ExprNode[] ParameterExpressions => _parameterExpressions;

        /// <summary>
        /// Returns the engine URI.
        /// </summary>
        /// <value>engine URI.</value>
        public string EngineURI => _engineURI;

        /// <summary>
        /// Returns the statement name.
        /// </summary>
        /// <value>statement name</value>
        public string StatementName => _statementName;

        /// <summary>
        /// Returns additional validation contextual services.
        /// </summary>
        /// <value>validation context</value>
        public ExprValidationContext ValidationContext => _validationContext;

        /// <summary>
        /// Returns the original configuration object for the aggregation multi-function
        /// </summary>
        /// <value>config</value>
        public ConfigurationPlugInAggregationMultiFunction Config => _config;

        public TableMetadataColumnAggregation OptionalTableColumnAccessed => _optionalTableColumnAccessed;

        /// <summary>
        /// Returns positional and non-positional parameters.
        /// </summary>
        /// <value>all parameters</value>
        public IList<ExprNode> AllParameterExpressions => _allParameterExpressions;

        public IDictionary<string, IList<ExprNode>> NamedParameters
        {
            get
            {
                var named = new Dictionary<string, IList<ExprNode>>();
                foreach (ExprNode node in _allParameterExpressions)
                {
                    if (node is ExprNamedParameterNode namedNode)
                    {
                        named.Put(namedNode.ParameterName, namedNode.ChildNodes);
                    }
                }

                return named;
            }
        }
    }
} // end of namespace
