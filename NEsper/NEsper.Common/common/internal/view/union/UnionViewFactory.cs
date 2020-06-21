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

namespace com.espertech.esper.common.@internal.view.union
{
    /// <summary>
    /// Factory for union-views.
    /// </summary>
    public class UnionViewFactory : ViewFactory,
        DataWindowViewFactory
    {
        protected EventType eventType;
        protected ViewFactory[] unioned;
        protected bool hasAsymetric;

        public void Init(
            ViewFactoryContext viewFactoryContext,
            EPStatementInitServices services)
        {
            foreach (ViewFactory factory in unioned) {
                factory.Init(viewFactoryContext, services);
            }
        }

        public View MakeView(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext)
        {
            IList<View> views = new List<View>();
            foreach (ViewFactory viewFactory in unioned) {
                views.Add(viewFactory.MakeView(agentInstanceViewFactoryContext));
            }

            if (hasAsymetric) {
                return new UnionAsymetricView(agentInstanceViewFactoryContext, this, views);
            }

            return new UnionView(agentInstanceViewFactoryContext, this, views);
        }

        public EventType EventType {
            get => eventType;
            set { this.eventType = value; }
        }

        public ViewFactory[] Unioned {
            get => unioned;
            set { this.unioned = value; }
        }

        public bool HasAsymetric {
            get => hasAsymetric;
            set { this.hasAsymetric = value; }
        }

        public string ViewName {
            get => "union";
        }
    }
} // end of namespace