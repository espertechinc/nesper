///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;

using com.espertech.esper.client;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.view.internals
{
    /// <summary>
    /// Factory for union-views.
    /// </summary>
    public class IntersectViewFactory
        : ViewFactory
        , DataWindowViewFactory
        , DataWindowViewFactoryUniqueCandidate
    {
        /// <summary>The event type. </summary>
        private EventType _parentEventType;

        /// <summary>The view factories. </summary>
        private IList<ViewFactory> _viewFactories;

        /// <summary>Ctor. </summary>
        public IntersectViewFactory()
        {
        }

        /// <summary>Sets the parent event type. </summary>
        /// <value>type</value>
        public EventType ParentEventType
        {
            get { return _parentEventType; }
            set { _parentEventType = value; }
        }

        /// <summary>Sets the view factories. </summary>
        /// <value>factories</value>
        public IList<ViewFactory> ViewFactories
        {
            get { return _viewFactories; }
            set
            {
                _viewFactories = value;
                int batchCount = 0;
                foreach (ViewFactory viewFactory in value)
                {
                    batchCount += viewFactory is DataWindowBatchingViewFactory ? 1 : 0;
                }
                if (batchCount > 1)
                {
                    throw new ViewProcessingException("Cannot combined multiple batch data windows into an intersection");
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
            bool hasAsymetric = false;
            bool hasBatch = false;
            foreach (ViewFactory viewFactory in _viewFactories)
            {
                agentInstanceViewFactoryContext.IsRemoveStream = true;
                views.Add(viewFactory.MakeView(agentInstanceViewFactoryContext));
                hasAsymetric |= viewFactory is AsymetricDataWindowViewFactory;
                hasBatch |= viewFactory is DataWindowBatchingViewFactory;
            }
            if (hasBatch)
            {
                return new IntersectBatchView(
                    agentInstanceViewFactoryContext, this, _parentEventType, views, _viewFactories, hasAsymetric);
            }
            else if (hasAsymetric)
            {
                return new IntersectAsymetricView(agentInstanceViewFactoryContext, this, _parentEventType, views);
            }
            return new IntersectView(agentInstanceViewFactoryContext, this, _parentEventType, views);
        }

        public EventType EventType
        {
            get { return _parentEventType; }
        }

        public bool CanReuse(View view)
        {
            return false;
        }

        public ICollection<string> UniquenessCandidatePropertyNames
        {
            get
            {
                foreach (ViewFactory viewFactory in _viewFactories)
                {
                    if (viewFactory is DataWindowViewFactoryUniqueCandidate)
                    {
                        var unique = (DataWindowViewFactoryUniqueCandidate) viewFactory;
                        var props = unique.UniquenessCandidatePropertyNames;
                        if (props != null)
                        {
                            return props;
                        }
                    }
                }
                return null;
            }
        }

        public string ViewName
        {
            get { return GetViewNameUnionIntersect(true, _viewFactories); }
        }

        internal static String GetViewNameUnionIntersect(bool intersect, ICollection<ViewFactory> factories)
        {
            var buf = new StringBuilder();
            buf.Append(intersect ? "Intersection" : "Union");

            if (factories == null)
            {
                return buf.ToString();
            }
            buf.Append(" of ");
            String delimiter = "";
            foreach (ViewFactory factory in factories)
            {
                buf.Append(delimiter);
                buf.Append(factory.ViewName);
                delimiter = ",";
            }

            return buf.ToString();
        }
    }
}
