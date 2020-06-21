///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.variable.core;

namespace com.espertech.esper.common.@internal.filterspec
{
    /// <summary>
    ///     Adapter to evaluate boolean expressions, providing
    ///     events per stream to expression nodes. Generated by @{link FilterSpecParamExprNode} for
    ///     boolean expression filter parameters.
    /// </summary>
    public sealed class ExprNodeAdapterMSNoTL : ExprNodeAdapterMSBase
    {
        private readonly VariableManagementService _variableService;

        internal ExprNodeAdapterMSNoTL(
            FilterSpecParamExprNode factory,
            ExprEvaluatorContext evaluatorContext,
            EventBean[] prototype,
            VariableManagementService variableService)
            : base(factory, evaluatorContext, prototype)
        {
            _variableService = variableService;
        }

        public override bool Evaluate(EventBean theEvent)
        {
            _variableService?.SetLocalVersion();

            var eventsPerStream = new EventBean[prototypeArray.Length];
            Array.Copy(prototypeArray, 0, eventsPerStream, 0, prototypeArray.Length);
            eventsPerStream[0] = theEvent;
            return EvaluatePerStream(eventsPerStream);
        }
    }
} // end of namespace