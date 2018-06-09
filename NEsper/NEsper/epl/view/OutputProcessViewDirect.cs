///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.view
{
    /// <summary>
    /// Output process view that does not enforce any output policies and may simply
    /// hand over events to child views, does not handle distinct.
    /// </summary>
    public class OutputProcessViewDirect : OutputProcessViewBase
    {
        private readonly OutputProcessViewDirectFactory _parent;
    
        public OutputProcessViewDirect(ResultSetProcessor resultSetProcessor, OutputProcessViewDirectFactory parent)
                    : base(resultSetProcessor)
        {
            _parent = parent;
        }

        public override int NumChangesetRows => 0;

        public override OutputCondition OptionalOutputCondition => null;

        public override OutputProcessViewConditionDeltaSet OptionalDeltaSet => null;

        public override OutputProcessViewAfterState OptionalAfterConditionState => null;

        /// <summary>The Update method is called if the view does not participate in a join. </summary>
        /// <param name="newData">new events</param>
        /// <param name="oldData">old events</param>
        public override void Update(EventBean[] newData, EventBean[] oldData)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QOutputProcessNonBuffered(newData, oldData);}
    
            bool isGenerateSynthetic = _parent.StatementResultService.IsMakeSynthetic;
            bool isGenerateNatural = _parent.StatementResultService.IsMakeNatural;
    
            UniformPair<EventBean[]> newOldEvents = ResultSetProcessor.ProcessViewResult(newData, oldData, isGenerateSynthetic);
    
            if ((!isGenerateSynthetic) && (!isGenerateNatural))
            {
                if (AuditPath.IsAuditEnabled) {
                    OutputStrategyUtil.IndicateEarlyReturn(_parent.StatementContext, newOldEvents);
                }
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AOutputProcessNonBuffered();}
                return;
            }

            bool forceOutput = 
                (newData == null) && 
                (oldData == null) &&
                ((newOldEvents == null) || (newOldEvents.First == null && newOldEvents.Second == null));

            // Child view can be null in replay from named window
            if (ChildView != null)
            {
                PostProcess(forceOutput, newOldEvents, ChildView);
            }
    
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AOutputProcessNonBuffered();}
        }

        /// <summary>This process (Update) method is for participation in a join. </summary>
        /// <param name="newEvents">new events</param>
        /// <param name="oldEvents">old events</param>
        /// <param name="exprEvaluatorContext"></param>
        public override void Process(ISet<MultiKey<EventBean>> newEvents, ISet<MultiKey<EventBean>> oldEvents, ExprEvaluatorContext exprEvaluatorContext)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QOutputProcessNonBufferedJoin(newEvents, oldEvents);}
    
            bool isGenerateSynthetic = _parent.StatementResultService.IsMakeSynthetic;
            bool isGenerateNatural = _parent.StatementResultService.IsMakeNatural;
    
            UniformPair<EventBean[]> newOldEvents = ResultSetProcessor.ProcessJoinResult(newEvents, oldEvents, isGenerateSynthetic);
    
            if ((!isGenerateSynthetic) && (!isGenerateNatural))
            {
                if (AuditPath.IsAuditEnabled) {
                    OutputStrategyUtil.IndicateEarlyReturn(_parent.StatementContext, newOldEvents);
                }
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AOutputProcessNonBufferedJoin();}
                return;
            }
    
            if (newOldEvents == null)
            {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AOutputProcessNonBufferedJoin();}
                return;
            }
    
            // Child view can be null in replay from named window
            if (ChildView != null)
            {
                PostProcess(false, newOldEvents, ChildView);
            }
    
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AOutputProcessNonBufferedJoin();}
        }
    
        protected virtual void PostProcess(bool force, UniformPair<EventBean[]> newOldEvents, UpdateDispatchView childView)
        {
            OutputStrategyUtil.Output(force, newOldEvents, childView);
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return OutputStrategyUtil.GetEnumerator(JoinExecutionStrategy, ResultSetProcessor, Parent, false);
        }
    
        public override void Terminated()
        {
            // Not applicable
        }

        public override void Stop()
        {
            // Not applicable
        }
    }
}
