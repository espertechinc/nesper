///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
using static com.espertech.esper.common.@internal.epl.index.advanced.index.quadtree.AdvancedIndexQuadTreeConstants;
using static com.espertech.esper.common.@internal.epl.index.advanced.index.service.AdvancedIndexEvaluationHelper;

namespace com.espertech.esper.common.@internal.epl.index.advanced.index.quadtree
{
    public class EventTableQuadTreeMXCIFImpl : EventTableQuadTree
    {
        private readonly EventTableOrganization organization;
        private readonly EventBean[] eventsPerStream = new EventBean[1];
        private readonly AdvancedIndexConfigStatementMXCIFQuadtree config;
        private readonly MXCIFQuadTree<object> quadTree;

        public EventTableQuadTreeMXCIFImpl(
            EventTableOrganization organization,
            AdvancedIndexConfigStatementMXCIFQuadtree config,
            MXCIFQuadTree<object> quadTree)
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
            return (ICollection<EventBean>) MXCIFQuadTreeRowIndexQuery.QueryRange(quadTree, x, y, width, height);
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
            foreach (EventBean added in events) {
                Add(added, exprEvaluatorContext);
            }
        }

        public void Remove(
            EventBean[] events,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            foreach (EventBean removed in events) {
                Remove(removed, exprEvaluatorContext);
            }
        }

        public void Add(
            EventBean @event,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            eventsPerStream[0] = @event;
            double x = EvalDoubleColumn(config.XEval, organization.IndexName, COL_X, eventsPerStream, true, exprEvaluatorContext);
            double y = EvalDoubleColumn(config.YEval, organization.IndexName, COL_Y, eventsPerStream, true, exprEvaluatorContext);
            double width = EvalDoubleColumn(config.WidthEval, organization.IndexName, COL_WIDTH, eventsPerStream, true, exprEvaluatorContext);
            double height = EvalDoubleColumn(config.HeightEval, organization.IndexName, COL_HEIGHT, eventsPerStream, true, exprEvaluatorContext);
            bool added = MXCIFQuadTreeRowIndexAdd.Add(x, y, width, height, @event, quadTree, organization.IsUnique, organization.IndexName);
            if (!added) {
                throw InvalidColumnValue(
                    organization.IndexName, "(x,y,width,height)", "(" + x + "," + y + "," + width + "," + height + ")",
                    "a value intersecting index bounding box (range-end-inclusive) " + quadTree.Root.Bb);
            }
        }

        public void Remove(
            EventBean @event,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            eventsPerStream[0] = @event;
            double x = EvalDoubleColumn(config.XEval, organization.IndexName, COL_X, eventsPerStream, false, exprEvaluatorContext);
            double y = EvalDoubleColumn(config.YEval, organization.IndexName, COL_Y, eventsPerStream, false, exprEvaluatorContext);
            double width = EvalDoubleColumn(config.WidthEval, organization.IndexName, COL_WIDTH, eventsPerStream, false, exprEvaluatorContext);
            double height = EvalDoubleColumn(config.HeightEval, organization.IndexName, COL_HEIGHT, eventsPerStream, false, exprEvaluatorContext);
            MXCIFQuadTreeRowIndexRemove.Remove(x, y, width, height, @event, quadTree);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<EventBean> GetEnumerator()
        {
            BoundingBox bb = quadTree.Root.Bb;
            ICollection<EventBean> events = QueryRange(bb.MinX, bb.MinY, bb.MaxX - bb.MinX, bb.MaxY - bb.MinY);
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
            return this.GetType().ToString();
        }

        public Type ProviderClass {
            get => this.GetType();
        }

        public int? NumberOfEvents {
            get => null;
        }

        public int NumKeys {
            get => -1;
        }

        public object Index {
            get => quadTree;
        }

        public EventTableOrganization Organization {
            get => organization;
        }
    }
} // end of namespace