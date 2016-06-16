///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.named;
using com.espertech.esper.epl.property;

namespace com.espertech.esper.core.context.activator
{
    public class ViewableActivatorNamedWindow : ViewableActivator
    {
        private readonly NamedWindowProcessor _processor;
        private readonly IList<ExprNode> _filterExpressions;
        private readonly PropertyEvaluator _optPropertyEvaluator;

        public ViewableActivatorNamedWindow(
            NamedWindowProcessor processor,
            IList<ExprNode> filterExpressions,
            PropertyEvaluator optPropertyEvaluator)
        {
            _processor = processor;
            _filterExpressions = filterExpressions;
            _optPropertyEvaluator = optPropertyEvaluator;
        }

        public ViewableActivationResult Activate(
            AgentInstanceContext agentInstanceContext,
            bool isSubselect,
            bool isRecoveringResilient)
        {
            var consumerDesc = new NamedWindowConsumerDesc(
                _filterExpressions, _optPropertyEvaluator, agentInstanceContext);
            var consumerView = _processor.AddConsumer(consumerDesc, isSubselect);
            return new ViewableActivationResult(consumerView, consumerView, null, null, null, false, false, null);
        }
    }
}