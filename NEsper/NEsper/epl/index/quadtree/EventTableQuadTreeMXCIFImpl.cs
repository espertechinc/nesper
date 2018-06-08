///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.index.service;
using com.espertech.esper.epl.join.table;
using com.espertech.esper.spatial.quadtree.mxcif;
using com.espertech.esper.spatial.quadtree.mxcifrowindex;

//import static com.espertech.esper.epl.index.quadtree.AdvancedIndexQuadTreeConstants.*;
//import static com.espertech.esper.epl.index.service.AdvancedIndexEvaluationHelper.evalDoubleColumn;
//import static com.espertech.esper.epl.index.service.AdvancedIndexEvaluationHelper.invalidColumnValue;

namespace com.espertech.esper.epl.index.quadtree
{
    public class EventTableQuadTreeMXCIFImpl : EventTableQuadTree
    {
        private readonly EventTableOrganization _organization;
        private readonly EventBean[] _eventsPerStream = new EventBean[1];
        private readonly AdvancedIndexConfigStatementMXCIFQuadtree _config;
        private readonly MXCIFQuadTree<Object> _quadTree;

        public EventTableQuadTreeMXCIFImpl(
            EventTableOrganization organization,
            AdvancedIndexConfigStatementMXCIFQuadtree config,
            MXCIFQuadTree<Object> quadTree)
        {
            _organization = organization;
            _config = config;
            _quadTree = quadTree;
        }

        public ICollection<EventBean> QueryRange(double x, double y, double width, double height)
        {
            return MXCIFQuadTreeRowIndexQuery.QueryRange(_quadTree, x, y, width, height)
                .Unwrap<EventBean>();
        }

        public void AddRemove(EventBean[] newData, EventBean[] oldData, ExprEvaluatorContext exprEvaluatorContext)
        {
            Remove(oldData, exprEvaluatorContext);
            Add(newData, exprEvaluatorContext);
        }

        public void Add(EventBean[] events, ExprEvaluatorContext exprEvaluatorContext)
        {
            foreach (var added in events)
            {
                Add(added, exprEvaluatorContext);
            }
        }

        public void Remove(EventBean[] events, ExprEvaluatorContext exprEvaluatorContext)
        {
            foreach (var removed in events)
            {
                Remove(removed, exprEvaluatorContext);
            }
        }

        public void Add(EventBean @event, ExprEvaluatorContext exprEvaluatorContext)
        {
            _eventsPerStream[0] = @event;
            var x = AdvancedIndexEvaluationHelper.EvalDoubleColumn(_config.XEval, _organization.IndexName,
                AdvancedIndexQuadTreeConstants.COL_X, _eventsPerStream, true,
                exprEvaluatorContext);
            var y = AdvancedIndexEvaluationHelper.EvalDoubleColumn(_config.YEval, _organization.IndexName,
                AdvancedIndexQuadTreeConstants.COL_Y, _eventsPerStream, true,
                exprEvaluatorContext);
            var width = AdvancedIndexEvaluationHelper.EvalDoubleColumn(_config.WidthEval, _organization.IndexName,
                AdvancedIndexQuadTreeConstants.COL_WIDTH, _eventsPerStream, true,
                exprEvaluatorContext);
            var height = AdvancedIndexEvaluationHelper.EvalDoubleColumn(_config.HeightEval, _organization.IndexName,
                AdvancedIndexQuadTreeConstants.COL_HEIGHT, _eventsPerStream,
                true, exprEvaluatorContext);
            bool added = MXCIFQuadTreeRowIndexAdd.Add(x, y, width, height, @event, _quadTree, _organization.IsUnique,
                _organization.IndexName);
            if (!added)
            {
                throw AdvancedIndexEvaluationHelper.InvalidColumnValue(_organization.IndexName, "(x,y,width,height)",
                    "(" + x + "," + y + "," + width + "," + height + ")",
                    "a value intersecting index bounding box (range-end-inclusive) " + _quadTree.Root.Bb);
            }
        }

        public void Remove(EventBean @event, ExprEvaluatorContext exprEvaluatorContext)
        {
            _eventsPerStream[0] = @event;
            var x = AdvancedIndexEvaluationHelper.EvalDoubleColumn(_config.XEval, _organization.IndexName,
                AdvancedIndexQuadTreeConstants.COL_X, _eventsPerStream, false,
                exprEvaluatorContext);
            var y = AdvancedIndexEvaluationHelper.EvalDoubleColumn(_config.YEval, _organization.IndexName,
                AdvancedIndexQuadTreeConstants.COL_Y, _eventsPerStream, false,
                exprEvaluatorContext);
            var width = AdvancedIndexEvaluationHelper.EvalDoubleColumn(_config.WidthEval, _organization.IndexName,
                AdvancedIndexQuadTreeConstants.COL_WIDTH, _eventsPerStream, false,
                exprEvaluatorContext);
            var height = AdvancedIndexEvaluationHelper.EvalDoubleColumn(_config.HeightEval, _organization.IndexName,
                AdvancedIndexQuadTreeConstants.COL_HEIGHT, _eventsPerStream,
                false, exprEvaluatorContext);
            MXCIFQuadTreeRowIndexRemove.Remove(x, y, width, height, @event, _quadTree);
        }

        public IEnumerator<EventBean> GetEnumerator()
        {
            var bb = _quadTree.Root.Bb;
            var events = QueryRange(bb.MinX, bb.MinY, bb.MaxX - bb.MinX, bb.MaxY - bb.MinY);
            return events.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool IsEmpty()
        {
            return false; // assumed non-empty
        }

        public void Clear()
        {
            _quadTree.Clear();
        }

        public void Destroy()
        {
        }

        public string ToQueryPlan()
        {
            return GetType().ToString();
        }

        public Type ProviderClass => GetType();

        public int? NumberOfEvents => null;

        public int NumKeys => -1;

        public object Index => _quadTree;

        public EventTableOrganization Organization => _organization;
    }
} // end of namespace
