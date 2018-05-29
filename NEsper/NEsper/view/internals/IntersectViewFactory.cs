///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;
using System.Text;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.threading;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.view.internals
{
    /// <summary>
    /// Factory for union-views.
    /// </summary>
    public class IntersectViewFactory
        : ViewFactory
        , DataWindowViewFactory
        , DataWindowViewFactoryUniqueCandidate
        , ViewFactoryContainer
    {
        private EventType _parentEventType;
        private IList<ViewFactory> _viewFactories;
        private int _batchViewIndex;
        private bool _isAsymmetric;
        private IThreadLocal<IntersectBatchViewLocalState> _batchViewLocalState;
        private IThreadLocal<IntersectDefaultViewLocalState> _defaultViewLocalState;
        private IThreadLocal<IntersectAsymetricViewLocalState> _asymetricViewLocalState;
        private readonly IThreadLocalManager _threadLocalManager;

        public IntersectViewFactory(IContainer container)
        {
            _threadLocalManager = container.ThreadLocalManager();
        }

        /// <summary>
        /// Sets the view factories.
        /// </summary>
        /// <value>factories</value>
        public IList<ViewFactory> ViewFactories
        {
            set
            {
                _viewFactories = value;

                var batchCount = 0;
                for (var i = 0; i < value.Count; i++)
                {
                    ViewFactory viewFactory = value[i];
                    _isAsymmetric |= viewFactory is AsymetricDataWindowViewFactory;
                    if (viewFactory is DataWindowBatchingViewFactory)
                    {
                        batchCount++;
                        _batchViewIndex = i;
                    }
                }
                if (batchCount > 1)
                {
                    throw new ViewProcessingException("Cannot combined multiple batch data windows into an intersection");
                }

                if (batchCount == 1)
                {
                    _batchViewLocalState = _threadLocalManager.Create(
                        () => new IntersectBatchViewLocalState(
                            new EventBean[value.Count][],
                            new EventBean[value.Count][]));
                }
                else if (_isAsymmetric)
                {
                    _asymetricViewLocalState = _threadLocalManager.Create(
                        () => new IntersectAsymetricViewLocalState(
                            new EventBean[value.Count][]));
                }
                else
                {
                    _defaultViewLocalState = _threadLocalManager.Create(
                        () => new IntersectDefaultViewLocalState(
                            new EventBean[value.Count][]));
                }
            }
        }

        public void SetViewParameters(ViewFactoryContext viewFactoryContext, IList<ExprNode> viewParameters)
        {
        }

        public void Attach(
            EventType parentEventType,
            StatementContext statementContext,
            ViewFactory optionalParentFactory,
            IList<ViewFactory> parentViewFactories)
        {
        }

        public View MakeView(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext)
        {
            IList<View> views = new List<View>();
            var hasBatch = false;
            foreach (var viewFactory in _viewFactories)
            {
                agentInstanceViewFactoryContext.IsRemoveStream = true;
                views.Add(viewFactory.MakeView(agentInstanceViewFactoryContext));
                hasBatch |= viewFactory is DataWindowBatchingViewFactory;
            }
            if (hasBatch)
            {
                return new IntersectBatchView(agentInstanceViewFactoryContext, this, views);
            }
            else if (_isAsymmetric)
            {
                return new IntersectAsymetricView(agentInstanceViewFactoryContext, this, views);
            }
            return new IntersectDefaultView(agentInstanceViewFactoryContext, this, views);
        }

        public EventType EventType
        {
            get { return _parentEventType; }
        }

        public bool CanReuse(View view, AgentInstanceContext agentInstanceContext)
        {
            return false;
        }

        public ICollection<string> UniquenessCandidatePropertyNames
        {
            get
            {
                return _viewFactories
                    .OfType<DataWindowViewFactoryUniqueCandidate>()
                    .Select(unique => unique.UniquenessCandidatePropertyNames)
                    .FirstOrDefault(props => props != null);
            }
        }

        public string ViewName
        {
            get { return GetViewNameUnionIntersect(true, _viewFactories); }
        }

        public ICollection<ViewFactory> ViewFactoriesContained
        {
            get { return _viewFactories; }
        }

        internal static string GetViewNameUnionIntersect(bool intersect, ICollection<ViewFactory> factories)
        {
            var buf = new StringBuilder();
            buf.Append(intersect ? "Intersection" : "Union");

            if (factories == null)
            {
                return buf.ToString();
            }

            buf.Append(" of ");
            var delimiter = "";
            foreach (var factory in factories)
            {
                buf.Append(delimiter);
                buf.Append(factory.ViewName);
                delimiter = ",";
            }

            return buf.ToString();
        }

        /// <summary>
        /// Sets the parent event type.
        /// </summary>
        /// <value>type</value>
        public EventType ParentEventType
        {
            get { return _parentEventType; }
            set { _parentEventType = value; }
        }

        public int BatchViewIndex
        {
            get { return _batchViewIndex; }
        }

        public bool IsAsymmetric()
        {
            return _isAsymmetric;
        }

        public IntersectBatchViewLocalState GetBatchViewLocalStatePerThread()
        {
            return _batchViewLocalState.GetOrCreate();
        }

        public IntersectDefaultViewLocalState GetDefaultViewLocalStatePerThread()
        {
            return _defaultViewLocalState.GetOrCreate();
        }

        public IntersectAsymetricViewLocalState GetAsymetricViewLocalStatePerThread()
        {
            return _asymetricViewLocalState.GetOrCreate();
        }
    }
} // end of namespace
