///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

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
    public class UnionViewFactory 
        : ViewFactory
        , DataWindowViewFactory
        , ViewFactoryContainer
    {
        /// <summary>The event type. </summary>
        private EventType _parentEventType;
    
        /// <summary>The view factories. </summary>
        private IList<ViewFactory> _viewFactories;
    
        /// <summary>Ctor. Dependencies injected after reflective instantiation. </summary>
        public UnionViewFactory()
        {
        }

        /// <summary>Sets the parent event type. </summary>
        /// <value>type</value>
        public EventType ParentEventType
        {
            set { _parentEventType = value; }
            get { return _parentEventType; }
        }

        /// <summary>Sets the view factories. </summary>
        /// <value>factories</value>
        public IList<ViewFactory> ViewFactories
        {
            set { _viewFactories = value; }
            get { return _viewFactories; }
        }

        public void SetViewParameters(ViewFactoryContext viewFactoryContext, IList<ExprNode> viewParameters)
        {
        }
    
        public void Attach(EventType parentEventType, StatementContext statementContext, ViewFactory optionalParentFactory, IList<ViewFactory> parentViewFactories)
        {
        }
    
        public View MakeView(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext)
        {
            bool hasAsymetric = false;
            IList<View> views = new List<View>();
            foreach (ViewFactory viewFactory in _viewFactories)
            {
                views.Add(viewFactory.MakeView(agentInstanceViewFactoryContext));
                hasAsymetric |= viewFactory is AsymetricDataWindowViewFactory;
            }
            if (hasAsymetric) {
                return new UnionAsymetricView(agentInstanceViewFactoryContext, this, _parentEventType, views);
            }
            return new UnionView(agentInstanceViewFactoryContext, this, _parentEventType, views);
        }

        public EventType EventType
        {
            get { return _parentEventType; }
        }

        public bool CanReuse(View view)
        {
            return false;
        }

        public string ViewName
        {
            get { return IntersectViewFactory.GetViewNameUnionIntersect(false, _viewFactories); }
        }

        public ICollection<ViewFactory> ViewFactoriesContained
        {
            get { return _viewFactories; }
        }
    }
}
