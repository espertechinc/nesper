///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.filterspec;

namespace com.espertech.esper.common.@internal.epl.pattern.guard
{
    public class ExpressionGuardFactory : GuardFactory
    {
        internal MatchedEventConvertor convertor;
        internal ExprEvaluator expression;

        public ExprEvaluator Expression {
            set => expression = value;
        }

        public MatchedEventConvertor Convertor {
            set => convertor = value;
        }

        public Guard MakeGuard(
            PatternAgentInstanceContext context,
            MatchedEventMap beginState,
            Quitable quitable,
            object guardState)
        {
            return new ExpressionGuard(convertor, expression, quitable);
        }
    }
} // end of namespace