///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.spatial.quadtree.core;
using com.espertech.esper.common.@internal.epl.spatial.quadtree.mxcif;
using com.espertech.esper.common.@internal.epl.spatial.quadtree.mxcifrowindex;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.epl.index.advanced.index.quadtree.AdvancedIndexQuadTreeConstants;
using static com.espertech.esper.common.@internal.epl.index.advanced.index.service.AdvancedIndexEvaluationHelper;

namespace com.espertech.esper.common.@internal.epl.index.advanced.index.quadtree
{
    public class EventTableQuadTreeMXCIFImpl : EventTableQuadTree
    {
        private readonly EventTableOrganization organization;
        private readonly EventBean[] eventsPerStream = new EventBean[1];
        private readonly AdvancedIndexConfigStatementMXCIFQuadtree config;
        private readonly MXCIFQuadTree quadTree;

        public EventTableQuadTreeMXCIFImpl(
            EventTableOrganization organization,
            AdvancedIndexConfigStatementMXCIFQuadtree config,
            MXCIFQuadTree quadTree)
        {
            this.organization = organization;
            this.config = config;
            this.quadTree = quadTree;
        }

        public ICollection<EventBean> QueryRange(
            double x,
            double y,
            double width,
            double height)
        {
            return MXCIFQuadTreeRowIndexQuery
                .QueryRange(quadTree, x, y, width, height)
                .Unwrap<EventBean>();
        }

        public void AddRemove(
            EventBean[] newData,
            EventBean[] oldData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            exprEvaluatorContext.InstrumentationProvider.QIndexAddRemove(this, newData, oldData);

            Remove(oldData, exprEvaluatorContext);
            Add(newData, exprEvaluatorContext);

            exprEvaluatorContext.InstrumentationProvider.AIndexAddRemove();
        }

        public void Add(
            EventBean[] events,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            foreach (var added in events) {
                Add(added, exprEvaluatorContext);
            }
        }

        public void Remove(
            EventBean[] events,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            foreach (var removed in events) {
                Remove(removed, exprEvaluatorContext);
            }
        }

        public void Add(
            EventBean @event,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            eventsPerStream[0] = @event;
            var x = EvalDoubleColumn(
                config.XEval,
                organization.IndexName,
                COL_X,
                eventsPerStream,
                true,
                exprEvaluatorContext);
            var y = EvalDoubleColumn(
                config.YEval,
                organization.IndexName,
                COL_Y,
                eventsPerStream,
                true,
                exprEvaluatorContext);
            var width = EvalDoubleColumn(
                config.WidthEval,
                organization.IndexName,
                COL_WIDTH,
                eventsPerStream,
                true,
                exprEvaluatorContext);
            var height = EvalDoubleColumn(
                config.HeightEval,
                organization.IndexName,
                COL_HEIGHT,
                eventsPerStream,
                true,
                exprEvaluatorContext);
            var added = MXCIFQuadTreeRowIndexAdd.Add(
                x,
                y,
                width,
                height,
                @event,
                quadTree,
                organization.IsUnique,
                organization.IndexName);
            if (!added) {
                throw InvalidColumnValue(
                    organization.IndexName,
                    "(X,Y,Width,Height)",
                    "(" +
                    x.RenderAny() +
                    "," +
                    y.RenderAny() +
                    "," +
                    width.RenderAny() +
                    "," +
                    height.RenderAny() +
                    ")",
                    "a value intersecting index bounding box (range-end-inclusive) " + quadTree.Root.Bb);
            }
        }

        public void Remove(
            EventBean @event,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            eventsPerStream[0] = @event;
            var x = EvalDoubleColumn(
                config.XEval,
                organization.IndexName,
                COL_X,
                eventsPerStream,
                false,
                exprEvaluatorContext);
            var y = EvalDoubleColumn(
                config.YEval,
                organization.IndexName,
                COL_Y,
                eventsPerStream,
                false,
                exprEvaluatorContext);
            var width = EvalDoubleColumn(
                config.WidthEval,
                organization.IndexName,
                COL_WIDTH,
                eventsPerStream,
                false,
                exprEvaluatorContext);
            var height = EvalDoubleColumn(
                config.HeightEval,
                organization.IndexName,
                COL_HEIGHT,
                eventsPerStream,
                false,
                exprEvaluatorContext);
            MXCIFQuadTreeRowIndexRemove.Remove(x, y, width, height, @event, quadTree);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<EventBean> GetEnumerator()
        {
            var bb = quadTree.Root.Bb;
            var events = QueryRange(bb.MinX, bb.MinY, bb.MaxX - bb.MinX, bb.MaxY - bb.MinY);
            return events.GetEnumerator();
        }

        public bool IsEmpty => false;

        public void Clear()
        {
            quadTree.Clear();
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

        public object Index => quadTree;

        public EventTableOrganization Organization => organization;
    }
} // end of namespace