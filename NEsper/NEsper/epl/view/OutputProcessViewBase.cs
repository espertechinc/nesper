///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.@join.@base;
using com.espertech.esper.util;
using com.espertech.esper.view;

namespace com.espertech.esper.epl.view
{
    public abstract class OutputProcessViewBase
        : View
        , JoinSetIndicator
        , OutputProcessViewTerminable
        , StopCallback
    {
        protected UpdateDispatchView ChildView;
        protected Viewable ParentView;

        public abstract OutputCondition OptionalOutputCondition { get; }
        public abstract void Stop();

        protected OutputProcessViewBase(ResultSetProcessor resultSetProcessor)
        {
            ResultSetProcessor = resultSetProcessor;
        }

        public virtual Viewable Parent
        {
            get => ParentView;
            set => ParentView = value;
        }

        public ResultSetProcessor ResultSetProcessor { get; protected set; }

        public abstract int NumChangesetRows { get; }

        public abstract void Update(EventBean[] newData, EventBean[] oldData);

        public abstract OutputProcessViewConditionDeltaSet OptionalDeltaSet { get; }
        public abstract OutputProcessViewAfterState OptionalAfterConditionState { get; }

        public View AddView(View view)
        {
            if (ChildView != null)
            {
                throw new IllegalStateException("Child view has already been supplied");
            }
            ChildView = (UpdateDispatchView) view;
            return this;
        }

        public virtual View[] Views
        {
            get
            {
                if (ChildView == null)
                {
                    return ViewSupport.EMPTY_VIEW_ARRAY;
                }
                return new View[]
                {
                    ChildView
                };
            }
        }

        public void RemoveAllViews()
        {
            ChildView = null;
        }

        public bool RemoveView(View view)
        {
            if (view != ChildView)
            {
                throw new IllegalStateException("Cannot remove child view, view has not been supplied");
            }
            ChildView = null;
            return true;
        }

        public virtual bool HasViews => ChildView != null;

        public virtual EventType EventType
        {
            get
            {
                EventType eventType = ResultSetProcessor.ResultEventType;
                return eventType ?? ParentView.EventType;
            }
        }

        /// <summary>
        /// For joins, supplies the join execution strategy that provides iteration over statement results.
        /// </summary>
        /// <value>executes joins including static (non-continuous) joins</value>
        public virtual JoinExecutionStrategy JoinExecutionStrategy { protected get; set; }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public abstract IEnumerator<EventBean> GetEnumerator();
        public abstract void Process(ISet<MultiKey<EventBean>> newEvents, ISet<MultiKey<EventBean>> oldEvents, ExprEvaluatorContext exprEvaluatorContext);
        public abstract void Terminated();
    }
}
