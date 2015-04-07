///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.visitor;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.epl.variable;
using com.espertech.esper.events;
using com.espertech.esper.pattern;

namespace com.espertech.esper.filter
{
	/// <summary>
	/// This class represents an arbitrary expression node returning a boolean value as a 
	/// filter parameter in an <seealso cref="FilterSpecCompiled" /> filter specification.
	/// </summary>
	[Serializable]
    public class FilterSpecParamExprNode : FilterSpecParam
	{
	    private readonly string _statementName;
	    private readonly ExprNode _exprNode;
        private readonly IDictionary<string, Pair<EventType, string>> _taggedEventTypes;
        private readonly IDictionary<string, Pair<EventType, string>> _arrayEventTypes;
	    [NonSerialized] private readonly EventAdapterService _eventAdapterService;
	    [NonSerialized] private readonly VariableService _variableService;
	    [NonSerialized] private readonly TableService _tableService;
	    private readonly bool _hasVariable;
	    private readonly bool _useLargeThreadingProfile;
	    private readonly bool _hasFilterStreamSubquery;
	    private readonly bool _hasTableAccess;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="lookupable">is the lookup-able</param>
        /// <param name="filterOperator">is expected to be the BOOLEAN_EXPR operator</param>
        /// <param name="exprNode">represents the boolean expression</param>
        /// <param name="taggedEventTypes">is null if the expression doesn't need other streams, or is filled with a ordered list of stream names and types</param>
        /// <param name="arrayEventTypes">is a map of name tags and event type per tag for repeat-expressions that generate an array of events</param>
        /// <param name="variableService">provides access to variables</param>
        /// <param name="tableService">The table service.</param>
        /// <param name="eventAdapterService">for creating event types and event beans</param>
        /// <param name="configurationInformation">The configuration information.</param>
        /// <param name="statementName">Name of the statement.</param>
        /// <param name="hasSubquery">if set to <c>true</c> [has subquery].</param>
        /// <param name="hasTableAccess">if set to <c>true</c> [has table access].</param>
        /// <exception cref="System.ArgumentException">Invalid filter operator for filter expression node</exception>
        /// <throws>IllegalArgumentException for illegal args</throws>
	    public FilterSpecParamExprNode(
	        FilterSpecLookupable lookupable,
	        FilterOperator filterOperator,
	        ExprNode exprNode,
	        IDictionary<string, Pair<EventType, string>> taggedEventTypes,
	        IDictionary<string, Pair<EventType, string>> arrayEventTypes,
	        VariableService variableService,
	        TableService tableService,
	        EventAdapterService eventAdapterService,
	        ConfigurationInformation configurationInformation,
	        string statementName,
	        bool hasSubquery,
	        bool hasTableAccess)
            : base(lookupable, filterOperator)
	    {
	        if (filterOperator != FilterOperator.BOOLEAN_EXPRESSION)
	        {
	            throw new ArgumentException("Invalid filter operator for filter expression node");
	        }
	        _exprNode = exprNode;
	        _taggedEventTypes = taggedEventTypes;
	        _arrayEventTypes = arrayEventTypes;
	        _variableService = variableService;
	        _tableService = tableService;
	        _eventAdapterService = eventAdapterService;
	        _useLargeThreadingProfile = configurationInformation.EngineDefaults.ExecutionConfig.ThreadingProfile == ConfigurationEngineDefaults.ThreadingProfile.LARGE;
	        _statementName = statementName;
	        _hasFilterStreamSubquery = hasSubquery;
	        _hasTableAccess = hasTableAccess;

	        var visitor = new ExprNodeVariableVisitor();
	        exprNode.Accept(visitor);
	        _hasVariable = visitor.HasVariables;
	    }

	    /// <summary>
	    /// Returns the expression node of the boolean expression this filter parameter represents.
	    /// </summary>
	    /// <value>expression node</value>
	    public ExprNode ExprNode
	    {
	        get { return _exprNode; }
	    }

	    /// <summary>
	    /// Returns the map of tag/stream names to event types that the filter expressions map use (for patterns)
	    /// </summary>
	    /// <value>map</value>
        public IDictionary<string, Pair<EventType, string>> TaggedEventTypes
	    {
	        get { return _taggedEventTypes; }
	    }

	    public override object GetFilterValue(MatchedEventMap matchedEvents, ExprEvaluatorContext exprEvaluatorContext)
	    {
	        EventBean[] events = null;

	        if ((_taggedEventTypes != null && !_taggedEventTypes.IsEmpty()) || (_arrayEventTypes != null && !_arrayEventTypes.IsEmpty()))
	        {
	            var size = 0;
	            size += (_taggedEventTypes != null) ? _taggedEventTypes.Count : 0;
	            size += (_arrayEventTypes != null) ? _arrayEventTypes.Count : 0;
	            events = new EventBean[size + 1];

	            var count = 1;
	            if (_taggedEventTypes != null)
	            {
	                foreach (var tag in _taggedEventTypes.Keys)
	                {
	                    events[count] = matchedEvents.GetMatchingEventByTag(tag);
	                    count++;
	                }
	            }

	            if (_arrayEventTypes != null)
	            {
	                foreach (var entry in _arrayEventTypes)
	                {
	                    var compositeEventType = entry.Value.First;
	                    events[count] = _eventAdapterService.AdapterForTypedMap(matchedEvents.MatchingEventsAsMap, compositeEventType);
	                    count++;
	                }
	            }
	        }

	        // handle table evaluator context
	        if (_hasTableAccess) {
	            exprEvaluatorContext = new ExprEvaluatorContextWTableAccess(exprEvaluatorContext, _tableService);
	        }

	        // non-pattern case
	        ExprNodeAdapterBase adapter;
	        if (events == null) {

	            // if a subquery is present in a filter stream acquire the agent instance lock
	            if (_hasFilterStreamSubquery) {
	                adapter = new ExprNodeAdapterBaseStmtLock(_statementName, _exprNode, exprEvaluatorContext, _variableService);
	            }
	            // no-variable no-prior event evaluation
	            else if (!_hasVariable) {
	                adapter = new ExprNodeAdapterBase(_statementName, _exprNode, exprEvaluatorContext);
	            }
	            else {
	                // with-variable no-prior event evaluation
	                adapter = new ExprNodeAdapterBaseVariables(_statementName, _exprNode, exprEvaluatorContext, _variableService);
	            }
	        }
	        else {
	            // pattern cases
	            var variableServiceToUse = _hasVariable == false ? null : _variableService;
	            if (_useLargeThreadingProfile) {
	                // no-threadlocal evaluation
	                // if a subquery is present in a pattern filter acquire the agent instance lock
	                if (_hasFilterStreamSubquery) {
	                    adapter = new ExprNodeAdapterMultiStreamNoTLStmtLock(_statementName, _exprNode, exprEvaluatorContext, variableServiceToUse, events);
	                }
	                else {
	                    adapter = new ExprNodeAdapterMultiStreamNoTL(_statementName, _exprNode, exprEvaluatorContext, variableServiceToUse, events);
	                }
	            }
	            else {
	                if (_hasFilterStreamSubquery) {
	                    adapter = new ExprNodeAdapterMultiStreamStmtLock(_statementName, _exprNode, exprEvaluatorContext, variableServiceToUse, events);
	                }
	                else {
	                    // evaluation with threadlocal cache
	                    adapter = new ExprNodeAdapterMultiStream(_statementName, _exprNode, exprEvaluatorContext, variableServiceToUse, events);
	                }
	            }
	        }

	        if (!_hasTableAccess) {
	            return adapter;
	        }

	        // handle table
	        return new ExprNodeAdapterBaseWTableAccess(_statementName, _exprNode, exprEvaluatorContext, adapter, _tableService);
	    }

	    public override string ToString()
	    {
	        return base.ToString() + "  exprNode=" + _exprNode;
	    }

	    public override bool Equals(object obj)
	    {
	        if (this == obj)
	        {
	            return true;
	        }

	        if (!(obj is FilterSpecParamExprNode))
	        {
	            return false;
	        }

	        var other = (FilterSpecParamExprNode) obj;
	        if (!base.Equals(other))
	        {
	            return false;
	        }

	        if (_exprNode != other._exprNode)
	        {
	            return false;
	        }

	        return true;
	    }

	    public override int GetHashCode()
	    {
	        var result = base.GetHashCode();
            result = 31 * result + _exprNode.GetHashCode();
	        return result;
	    }
	}
} // end of namespace
