///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.view.derived
{
    /// <summary>
    ///     Factory for <seealso cref="WeightedAverageView" /> instances.
    /// </summary>
    public class WeightedAverageViewFactory : ViewFactory
    {
        protected internal StatViewAdditionalPropsEval additionalProps;

        protected internal EventType eventType;
        protected internal ExprEvaluator fieldNameWeightEvaluator;
        protected internal ExprEvaluator fieldNameXEvaluator;

        public ExprEvaluator FieldNameXEvaluator {
            get => fieldNameXEvaluator;
            set => fieldNameXEvaluator = value;
        }

        public ExprEvaluator FieldNameWeightEvaluator {
            get => fieldNameWeightEvaluator;
            set => fieldNameWeightEvaluator = value;
        }

        public StatViewAdditionalPropsEval AdditionalProps {
            get => additionalProps;
            set => additionalProps = value;
        }

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
            return new WeightedAverageView(this, agentInstanceViewFactoryContext);
        }

        public EventType EventType {
            get => eventType;
            set => eventType = value;
        }

        public string ViewName => ViewEnum.WEIGHTED_AVERAGE.Name;
    }
} // end of namespace