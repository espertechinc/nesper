///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat.threading;

namespace com.espertech.esper.common.@internal.view.intersect
{
    /// <summary>
    ///     Factory for union-views.
    /// </summary>
    public class IntersectViewFactory : ViewFactory,
        DataWindowViewFactory
    {
        protected internal IThreadLocal<IntersectAsymetricViewLocalState> asymetricViewLocalState;
        protected internal int batchViewIndex;
        protected internal IThreadLocal<IntersectBatchViewLocalState> batchViewLocalState;
        protected internal IThreadLocal<IntersectDefaultViewLocalState> defaultViewLocalState;
        protected internal EventType eventType;
        protected internal bool hasAsymetric;
        protected internal ViewFactory[] intersecteds;

        public int BatchViewIndex {
            get => batchViewIndex;
            set => batchViewIndex = value;
        }

        public bool IsAsymetric => hasAsymetric;

        public IntersectBatchViewLocalState BatchViewLocalStatePerThread => batchViewLocalState.GetOrCreate();

        public IntersectDefaultViewLocalState DefaultViewLocalStatePerThread => defaultViewLocalState.GetOrCreate();

        public IntersectAsymetricViewLocalState AsymetricViewLocalStatePerThread =>
            asymetricViewLocalState.GetOrCreate();

        public bool HasAsymetric {
            set => hasAsymetric = value;
        }

        public ViewFactory[] Intersecteds {
            get => intersecteds;
            set => intersecteds = value;
        }

        public void Init(
            ViewFactoryContext viewFactoryContext,
            EPStatementInitServices services)
        {
            foreach (var grouped in intersecteds) {
                grouped.Init(viewFactoryContext, services);
            }

            if (batchViewIndex != -1) {
                batchViewLocalState = new FastThreadLocal<IntersectBatchViewLocalState>(
                    () =>
                        new IntersectBatchViewLocalState(
                            new EventBean[intersecteds.Length][], new EventBean[intersecteds.Length][]));
            }
            else if (hasAsymetric) {
                asymetricViewLocalState = new FastThreadLocal<IntersectAsymetricViewLocalState>(
                    () =>
                        new IntersectAsymetricViewLocalState(new EventBean[intersecteds.Length][]));
            }
            else {
                defaultViewLocalState = new FastThreadLocal<IntersectDefaultViewLocalState>(
                    () =>
                        new IntersectDefaultViewLocalState(new EventBean[intersecteds.Length][]));
            }
        }

        public View MakeView(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext)
        {
            IList<View> views = new List<View>();
            foreach (var viewFactory in intersecteds) {
                agentInstanceViewFactoryContext.IsRemoveStream = true;
                views.Add(viewFactory.MakeView(agentInstanceViewFactoryContext));
            }

            if (batchViewIndex != -1) {
                return new IntersectBatchView(agentInstanceViewFactoryContext, this, views);
            }

            if (hasAsymetric) {
                return new IntersectAsymetricView(agentInstanceViewFactoryContext, this, views);
            }

            return new IntersectDefaultView(agentInstanceViewFactoryContext, this, views);
        }

        public EventType EventType {
            get => eventType;
            set => eventType = value;
        }

        public string ViewName => "intersect";

        public void SetEventType(EventType eventType)
        {
            this.eventType = eventType;
        }
    }
} // end of namespace