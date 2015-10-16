///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.spec;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.events;
using com.espertech.esper.view;

namespace com.espertech.esper.support.epl
{
    public class SupportResultSetProcessor : ResultSetProcessor
    {
        public ResultSetProcessor Copy(AgentInstanceContext agentInstanceContext)
        {
            return null;
        }

        public EventType ResultEventType
        {
            get { return SupportEventTypeFactory.CreateBeanType(typeof(SupportBean)); }
        }

        public UniformPair<EventBean[]> ProcessViewResult(EventBean[] newData, EventBean[] oldData, bool isSynthesize)
        {
            return new UniformPair<EventBean[]>(newData, oldData);
        }

        public UniformPair<EventBean[]> ProcessJoinResult(ISet<MultiKey<EventBean>> newEvents, ISet<MultiKey<EventBean>> oldEvents, bool isSynthesize)
        {
            return new UniformPair<EventBean[]>(newEvents.First().Array, oldEvents.First().Array);
        }

        public IEnumerator<EventBean> GetEnumerator(Viewable parent)
        {
            return null;
        }

        public IEnumerator<EventBean> GetEnumerator(ISet<MultiKey<EventBean>> joinSet)
        {
            return null;
        }

        public void Clear()
        {
        }

        public UniformPair<EventBean[]> ProcessOutputLimitedJoin(IList<UniformPair<ISet<MultiKey<EventBean>>>> joinEventsSet, bool generateSynthetic, OutputLimitLimitType outputLimitLimitType)
        {
            return null;
        }

        public UniformPair<EventBean[]> ProcessOutputLimitedView(IList<UniformPair<EventBean[]>> viewEventsList, bool generateSynthetic, OutputLimitLimitType outputLimitLimitType)
        {
            return null;
        }

        public bool HasAggregation
        {
            get { return false; }
        }

        public AgentInstanceContext AgentInstanceContext
        {
            set { }
        }

        public void ApplyViewResult(EventBean[] newData, EventBean[] oldData)
        {
        }

        public void ApplyJoinResult(ISet<MultiKey<EventBean>> newEvents, ISet<MultiKey<EventBean>> oldEvents)
        {
        }

        public void ProcessOutputLimitedLastAllNonBufferedView(EventBean[] newData, EventBean[] oldData, bool isGenerateSynthetic, bool isAll)
        {
        }

        public void ProcessOutputLimitedLastAllNonBufferedJoin(ISet<MultiKey<EventBean>> newEvents, ISet<MultiKey<EventBean>> oldEvents, bool isGenerateSynthetic, bool isAll)
        {
        }

        public UniformPair<EventBean[]> ContinueOutputLimitedLastAllNonBufferedView(bool isSynthesize, bool isAll)
        {
            return null;
        }

        public UniformPair<EventBean[]> ContinueOutputLimitedLastAllNonBufferedJoin(bool isSynthesize, bool isAll)
        {
            return null;
        }
    }
}
