///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.view.core;

namespace com.espertech.esper.common.@internal.view.firstunique
{
    /// <summary>
    ///     Factory for <seealso cref="FirstUniqueByPropertyView" /> instances.
    /// </summary>
    public class FirstUniqueByPropertyViewFactory : ViewFactory
    {
        protected ExprEvaluator[] criteriaEvals;
        protected Type[] criteriaTypes;
        protected EventType eventType;

        public ExprEvaluator[] CriteriaEvals {
            get => criteriaEvals;
            set => criteriaEvals = value;
        }

        public Type[] CriteriaTypes {
            set => criteriaTypes = value;
        }

        public void Init(ViewFactoryContext viewFactoryContext, EPStatementInitServices services)
        {
        }

        public View MakeView(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext)
        {
            return new FirstUniqueByPropertyView(this, agentInstanceViewFactoryContext);
        }

        public EventType EventType {
            get => eventType;
            set => eventType = value;
        }

        public string ViewName => ViewEnum.UNIQUE_FIRST_BY_PROPERTY.Name;
    }
} // end of namespace