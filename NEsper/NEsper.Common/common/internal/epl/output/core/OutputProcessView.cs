///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.join.@base;
using com.espertech.esper.common.@internal.epl.output.condition;
using com.espertech.esper.common.@internal.statement.dispatch;
using com.espertech.esper.common.@internal.view.core;

namespace com.espertech.esper.common.@internal.epl.output.core
{
    public abstract class OutputProcessView : View,
        JoinSetIndicator,
        AgentInstanceStopCallback,
        OutputProcessViewTerminable
    {
        protected internal UpdateDispatchView child;
        protected internal JoinExecutionStrategy joinExecutionStrategy;
        protected internal Viewable parentView;

        public JoinExecutionStrategy JoinExecutionStrategy {
            get => joinExecutionStrategy;
            set => joinExecutionStrategy = value;
        }

        public abstract int NumChangesetRows { get; }

        public abstract OutputCondition OptionalOutputCondition { get; }

        public Viewable Parent {
            get => parentView;
            set => parentView = value;
        }

        public View Child {
            get => child;
            set => child = (UpdateDispatchView) value;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public abstract IEnumerator<EventBean> GetEnumerator();
        public abstract EventType EventType { get; }

        public abstract void Update(
            EventBean[] newData,
            EventBean[] oldData);

        public abstract void Process(
            ISet<MultiKey<EventBean>> newEvents,
            ISet<MultiKey<EventBean>> oldEvents,
            ExprEvaluatorContext exprEvaluatorContext);

        public abstract void Stop(AgentInstanceStopServices services);
        public abstract void Terminated();
    }
} // end of namespace