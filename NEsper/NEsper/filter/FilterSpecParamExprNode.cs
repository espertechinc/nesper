///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.visitor;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.epl.variable;
using com.espertech.esper.events;
using com.espertech.esper.pattern;

namespace com.espertech.esper.filter
{
    /// <summary>
    /// This class represents an arbitrary expression node returning a boolean value as
    /// a filter parameter in an <seealso cref="FilterSpecCompiled" /> filter specification.
    /// </summary>
    public sealed class FilterSpecParamExprNode : FilterSpecParam
    {
        private readonly ExprNode _exprNode;
        private readonly IDictionary<string, Pair<EventType, string>> _taggedEventTypes;
        private readonly IDictionary<string, Pair<EventType, string>> _arrayEventTypes;
        [NonSerialized] private readonly EventAdapterService _eventAdapterService;
        [NonSerialized] private readonly FilterBooleanExpressionFactory _filterBooleanExpressionFactory;
        [NonSerialized] private readonly VariableService _variableService;
        [NonSerialized] private readonly TableService _tableService;
        private readonly bool _hasVariable;
        private readonly bool _useLargeThreadingProfile;
        private readonly bool _hasFilterStreamSubquery;
        private readonly bool _hasTableAccess;
        private int _filterSpecId;
        private int _filterSpecParamPathNum;

        public FilterSpecParamExprNode(
            FilterSpecLookupable lookupable,
            FilterOperator filterOperator,
            ExprNode exprNode,
            IDictionary<string, Pair<EventType, string>> taggedEventTypes,
            IDictionary<string, Pair<EventType, string>> arrayEventTypes,
            VariableService variableService,
            TableService tableService,
            EventAdapterService eventAdapterService,
            FilterBooleanExpressionFactory filterBooleanExpressionFactory,
            ConfigurationInformation configurationInformation,
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
            _filterBooleanExpressionFactory = filterBooleanExpressionFactory;
            _useLargeThreadingProfile = configurationInformation.EngineDefaults.Execution.ThreadingProfile ==
                                        ConfigurationEngineDefaults.ThreadingProfile.LARGE;
            _hasFilterStreamSubquery = hasSubquery;
            _hasTableAccess = hasTableAccess;

            var visitor = new ExprNodeVariableVisitor(variableService);
            exprNode.Accept(visitor);
            _hasVariable = visitor.HasVariables;
        }

        /// <summary>
        /// Returns the expression node of the bool expression this filter parameter represents.
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

        public override object GetFilterValue(
            MatchedEventMap matchedEvents,
            AgentInstanceContext agentInstanceContext)
        {
            EventBean[] events = null;

            if ((_taggedEventTypes != null && !_taggedEventTypes.IsEmpty()) ||
                (_arrayEventTypes != null && !_arrayEventTypes.IsEmpty()))
            {
                int size = 0;
                size += (_taggedEventTypes != null) ? _taggedEventTypes.Count : 0;
                size += (_arrayEventTypes != null) ? _arrayEventTypes.Count : 0;
                events = new EventBean[size + 1];

                int count = 1;
                if (_taggedEventTypes != null)
                {
                    foreach (string tag in _taggedEventTypes.Keys)
                    {
                        events[count] = matchedEvents.GetMatchingEventByTag(tag);
                        count++;
                    }
                }

                if (_arrayEventTypes != null)
                {
                    foreach (var entry in _arrayEventTypes)
                    {
                        EventType compositeEventType = entry.Value.First;
                        events[count] = _eventAdapterService.AdapterForTypedMap(
                            matchedEvents.MatchingEventsAsMap, compositeEventType);
                        count++;
                    }
                }
            }

            return _filterBooleanExpressionFactory.Make(
                this, events, agentInstanceContext, agentInstanceContext.StatementContext,
                agentInstanceContext.AgentInstanceId);
        }

        public override String ToString()
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

            FilterSpecParamExprNode other = (FilterSpecParamExprNode) obj;
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
            int result = base.GetHashCode();
            result = 31*result + _exprNode.GetHashCode();
            return result;
        }

        public int FilterSpecId
        {
            get { return _filterSpecId; }
            set { _filterSpecId = value; }
        }

        public int FilterSpecParamPathNum
        {
            get { return _filterSpecParamPathNum; }
            set { _filterSpecParamPathNum = value; }
        }

        public IDictionary<string, Pair<EventType, string>> ArrayEventTypes
        {
            get { return _arrayEventTypes; }
        }

        public EventAdapterService EventAdapterService
        {
            get { return _eventAdapterService; }
        }

        public FilterBooleanExpressionFactory FilterBooleanExpressionFactory
        {
            get { return _filterBooleanExpressionFactory; }
        }

        public VariableService VariableService
        {
            get { return _variableService; }
        }

        public TableService TableService
        {
            get { return _tableService; }
        }

        public bool HasVariable
        {
            get { return _hasVariable; }
        }

        public bool UseLargeThreadingProfile
        {
            get { return _useLargeThreadingProfile; }
        }

        public bool HasFilterStreamSubquery
        {
            get { return _hasFilterStreamSubquery; }
        }

        public bool HasTableAccess
        {
            get { return _hasTableAccess; }
        }
    }
} // end of namespace
