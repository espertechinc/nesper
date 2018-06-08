///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.@join.@base;
using com.espertech.esper.epl.join.plan;
using com.espertech.esper.epl.named;
using com.espertech.esper.filter;
using com.espertech.esper.view;

namespace com.espertech.esper.core.context.factory
{
    public class StatementAgentInstancePostLoadSelect : StatementAgentInstancePostLoad
    {
        private readonly Viewable[] _streamViews;
        private readonly JoinSetComposerDesc _joinSetComposer;
        private readonly NamedWindowTailViewInstance[] _namedWindowTailViews;
        private readonly QueryGraph[] _namedWindowPostloadFilters;
        private readonly IList<ExprNode>[] _namedWindowFilters;
        private readonly Attribute[] _annotations;
        private readonly ExprEvaluatorContext _exprEvaluatorContext;

        public StatementAgentInstancePostLoadSelect(
            Viewable[] streamViews,
            JoinSetComposerDesc joinSetComposer,
            NamedWindowTailViewInstance[] namedWindowTailViews,
            QueryGraph[] namedWindowPostloadFilters,
            IList<ExprNode>[] namedWindowFilters,
            Attribute[] annotations,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            _streamViews = streamViews;
            _joinSetComposer = joinSetComposer;
            _namedWindowTailViews = namedWindowTailViews;
            _namedWindowPostloadFilters = namedWindowPostloadFilters;
            _namedWindowFilters = namedWindowFilters;
            _annotations = annotations;
            _exprEvaluatorContext = exprEvaluatorContext;
        }

        public void ExecutePostLoad()
        {
            if ((_joinSetComposer == null) || (!_joinSetComposer.JoinSetComposer.AllowsInit))
            {
                return;
            }
            var events = new EventBean[_streamViews.Length][];
            for (var stream = 0; stream < _streamViews.Length; stream++)
            {
                var streamView = _streamViews[stream];
                if (streamView is HistoricalEventViewable)
                {
                    continue;
                }

                ICollection<EventBean> eventsInWindow;
                if (_namedWindowTailViews[stream] != null)
                {
                    var nwtail = _namedWindowTailViews[stream];
                    var snapshot = nwtail.SnapshotNoLock(
                        _namedWindowPostloadFilters[stream], _annotations);
                    if (_namedWindowFilters[stream] != null)
                    {
                        eventsInWindow = new List<EventBean>(snapshot.Count);
                        ExprNodeUtility.ApplyFilterExpressionsIterable(
                            snapshot, _namedWindowFilters[stream], _exprEvaluatorContext, eventsInWindow);
                    }
                    else
                    {
                        eventsInWindow = snapshot;
                    }
                }
                else if (_namedWindowFilters[stream] != null && !_namedWindowFilters[stream].IsEmpty())
                {
                    eventsInWindow = new LinkedList<EventBean>();
                    ExprNodeUtility.ApplyFilterExpressionsIterable(
                        _streamViews[stream], _namedWindowFilters[stream], _exprEvaluatorContext, eventsInWindow);
                }
                else
                {
                    eventsInWindow = new ArrayDeque<EventBean>();
                    foreach (var aConsumerView in _streamViews[stream])
                    {
                        eventsInWindow.Add(aConsumerView);
                    }
                }
                events[stream] = eventsInWindow.ToArray();
            }
            _joinSetComposer.JoinSetComposer.Init(events, _exprEvaluatorContext);
        }

        public void AcceptIndexVisitor(StatementAgentInstancePostLoadIndexVisitor visitor)
        {
            // no action
        }
    }
}
