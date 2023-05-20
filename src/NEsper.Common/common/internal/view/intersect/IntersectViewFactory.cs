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
using com.espertech.esper.compat.threading.threadlocal;

namespace com.espertech.esper.common.@internal.view.intersect
{
    /// <summary>
    ///     Factory for union-views.
    /// </summary>
    public class IntersectViewFactory : ViewFactory,
        DataWindowViewFactory
    {
        private IThreadLocal<IntersectAsymetricViewLocalState> _asymetricViewLocalState;
        private int _batchViewIndex;
        private IThreadLocal<IntersectBatchViewLocalState> _batchViewLocalState;
        private IThreadLocal<IntersectDefaultViewLocalState> _defaultViewLocalState;
        private EventType _eventType;
        private bool _hasAsymetric;
        private ViewFactory[] _intersecteds;

        public int BatchViewIndex {
            get => _batchViewIndex;
            set => _batchViewIndex = value;
        }

        public bool IsAsymetric => _hasAsymetric;

        public IntersectBatchViewLocalState BatchViewLocalStatePerThread => _batchViewLocalState.GetOrCreate();

        public IntersectDefaultViewLocalState DefaultViewLocalStatePerThread => _defaultViewLocalState.GetOrCreate();

        public IntersectAsymetricViewLocalState AsymetricViewLocalStatePerThread =>
            _asymetricViewLocalState.GetOrCreate();

        public bool HasAsymetric {
            set => _hasAsymetric = value;
        }

        public ViewFactory[] Intersecteds {
            get => _intersecteds;
            set => _intersecteds = value;
        }

        public void Init(
            ViewFactoryContext viewFactoryContext,
            EPStatementInitServices services)
        {
            foreach (var grouped in _intersecteds) {
                grouped.Init(viewFactoryContext, services);
            }

            if (_batchViewIndex != -1) {
                _batchViewLocalState = new SystemThreadLocal<IntersectBatchViewLocalState>(
                    () =>
                        new IntersectBatchViewLocalState(
                            new EventBean[_intersecteds.Length][],
                            new EventBean[_intersecteds.Length][]));
            }
            else if (_hasAsymetric) {
                _asymetricViewLocalState = new SystemThreadLocal<IntersectAsymetricViewLocalState>(
                    () => new IntersectAsymetricViewLocalState(new EventBean[_intersecteds.Length][]));
            }
            else {
                _defaultViewLocalState = new SystemThreadLocal<IntersectDefaultViewLocalState>(
                    () => new IntersectDefaultViewLocalState(new EventBean[_intersecteds.Length][]));
            }
        }

        public View MakeView(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext)
        {
            IList<View> views = new List<View>();
            foreach (var viewFactory in _intersecteds) {
                agentInstanceViewFactoryContext.IsRemoveStream = true;
                views.Add(viewFactory.MakeView(agentInstanceViewFactoryContext));
            }

            if (_batchViewIndex != -1) {
                return new IntersectBatchView(agentInstanceViewFactoryContext, this, views);
            }

            if (_hasAsymetric) {
                return new IntersectAsymetricView(agentInstanceViewFactoryContext, this, views);
            }

            return new IntersectDefaultView(agentInstanceViewFactoryContext, this, views);
        }

        public EventType EventType {
            get => _eventType;
            set => _eventType = value;
        }

        public string ViewName => "intersect";
    }
} // end of namespace