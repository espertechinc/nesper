///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.resultset.handthru
{
    public class ResultSetProcessorHandThroughImpl : ResultSetProcessor
    {
        private readonly ResultSetProcessorHandThroughFactory factory;
        private ExprEvaluatorContext exprEvaluatorContext;

        public ResultSetProcessorHandThroughImpl(
            ResultSetProcessorHandThroughFactory factory,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            this.factory = factory;
            this.exprEvaluatorContext = exprEvaluatorContext;
        }

        public UniformPair<EventBean[]> ProcessViewResult(
            EventBean[] newData,
            EventBean[] oldData,
            bool isSynthesize)
        {
            EventBean[] selectOldEvents = null;
            if (factory.IsRstream) {
                selectOldEvents = ResultSetProcessorHandThroughUtil.GetSelectEventsNoHavingHandThruView(
                    factory.SelectExprProcessor,
                    oldData,
                    false,
                    isSynthesize,
                    exprEvaluatorContext);
            }

            var selectNewEvents = ResultSetProcessorHandThroughUtil.GetSelectEventsNoHavingHandThruView(
                factory.SelectExprProcessor,
                newData,
                true,
                isSynthesize,
                exprEvaluatorContext);
            return new UniformPair<EventBean[]>(selectNewEvents, selectOldEvents);
        }

        public UniformPair<EventBean[]> ProcessJoinResult(
            ISet<MultiKeyArrayOfKeys<EventBean>> newEvents,
            ISet<MultiKeyArrayOfKeys<EventBean>> oldEvents,
            bool isSynthesize)
        {
            EventBean[] selectOldEvents = null;
            if (factory.IsRstream) {
                selectOldEvents = ResultSetProcessorHandThroughUtil.GetSelectEventsNoHavingHandThruJoin(
                    factory.SelectExprProcessor,
                    oldEvents,
                    false,
                    isSynthesize,
                    exprEvaluatorContext);
            }

            var selectNewEvents = ResultSetProcessorHandThroughUtil.GetSelectEventsNoHavingHandThruJoin(
                factory.SelectExprProcessor,
                newEvents,
                true,
                isSynthesize,
                exprEvaluatorContext);
            return new UniformPair<EventBean[]>(selectNewEvents, selectOldEvents);
        }

        public IEnumerator<EventBean> GetEnumerator(Viewable viewable)
        {
            return new TransformEventEnumerator(viewable.GetEnumerator(), new ResultSetProcessorHandtruTransform(this));
        }

        public IEnumerator<EventBean> GetEnumerator(ISet<MultiKeyArrayOfKeys<EventBean>> joinSet)
        {
            var result = ProcessJoinResult(joinSet, EmptySet<MultiKeyArrayOfKeys<EventBean>>.Instance, true);
            return new ArrayEventEnumerator(result.First);
        }

        public void Clear()
        {
        }

        public void Stop()
        {
        }

        public UniformPair<EventBean[]> ProcessOutputLimitedJoin(
            IList<UniformPair<ISet<MultiKeyArrayOfKeys<EventBean>>>> joinEventsSet,
            bool generateSynthetic)
        {
            throw new UnsupportedOperationException();
        }

        public UniformPair<EventBean[]> ProcessOutputLimitedView(
            IList<UniformPair<EventBean[]>> viewEventsList,
            bool generateSynthetic)
        {
            throw new UnsupportedOperationException();
        }

        public void ApplyViewResult(
            EventBean[] newData,
            EventBean[] oldData)
        {
            // not implemented
        }

        public void ApplyJoinResult(
            ISet<MultiKeyArrayOfKeys<EventBean>> newEvents,
            ISet<MultiKeyArrayOfKeys<EventBean>> oldEvents)
        {
            // not implemented
        }

        public void ProcessOutputLimitedLastAllNonBufferedView(
            EventBean[] newData,
            EventBean[] oldData,
            bool isSynthesize)
        {
            // not implemented
        }

        public void ProcessOutputLimitedLastAllNonBufferedJoin(
            ISet<MultiKeyArrayOfKeys<EventBean>> newEvents,
            ISet<MultiKeyArrayOfKeys<EventBean>> oldEvents,
            bool isGenerateSynthetic)
        {
            // not implemented
        }

        public UniformPair<EventBean[]> ContinueOutputLimitedLastAllNonBufferedView(bool isSynthesize)
        {
            throw new UnsupportedOperationException();
        }

        public UniformPair<EventBean[]> ContinueOutputLimitedLastAllNonBufferedJoin(bool isSynthesize)
        {
            throw new UnsupportedOperationException();
        }

        public void AcceptHelperVisitor(ResultSetProcessorOutputHelperVisitor visitor)
        {
        }

        public EventType ResultEventType => factory.ResultEventType;

        public ExprEvaluatorContext ExprEvaluatorContext {
            get => exprEvaluatorContext;
            set => exprEvaluatorContext = value;
        }
    }
} // end of namespace