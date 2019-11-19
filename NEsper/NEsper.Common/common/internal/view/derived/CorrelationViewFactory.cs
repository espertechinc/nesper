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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.view.derived
{
    /// <summary>
    /// Factory for <seealso cref="CorrelationView" /> instances.
    /// </summary>
    public class CorrelationViewFactory : ViewFactory
    {
        protected ExprEvaluator expressionXEval;
        protected ExprEvaluator expressionYEval;
        protected StatViewAdditionalPropsEval additionalProps;
        protected EventType eventType;

        public void Init(
            ViewFactoryContext viewFactoryContext,
            EPStatementInitServices services)
        {
            if (eventType == null) {
                throw new IllegalStateException("Event type not provided");
            }
        }

        public View MakeView(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext)
        {
            return new CorrelationView(
                this,
                agentInstanceViewFactoryContext.AgentInstanceContext,
                expressionXEval,
                expressionYEval,
                eventType,
                additionalProps);
        }

        public EventType EventType {
            get => eventType;
            set { this.eventType = value; }
        }

        public StatViewAdditionalPropsEval AdditionalProps {
            get => additionalProps;
            set { this.additionalProps = value; }
        }

        public ExprEvaluator ExpressionXEval {
            get => expressionXEval;
            set { this.expressionXEval = value; }
        }

        public ExprEvaluator ExpressionYEval {
            get => expressionYEval;
            set { this.expressionYEval = value; }
        }

        public string ViewName {
            get => ViewEnum.CORRELATION.GetViewName();
        }
    }
} // end of namespace