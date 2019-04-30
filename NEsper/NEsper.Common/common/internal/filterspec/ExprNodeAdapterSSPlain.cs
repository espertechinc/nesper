///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.filterspec
{
    public sealed class ExprNodeAdapterSSPlain : ExprNodeAdapterBase
    {
        internal ExprNodeAdapterSSPlain(
            FilterSpecParamExprNode factory,
            ExprEvaluatorContext evaluatorContext)
            : base(factory, evaluatorContext)
        {
        }

        public override bool Evaluate(EventBean theEvent)
        {
            return EvaluatePerStream(new[] {theEvent});
        }
    }
} // end of namespace