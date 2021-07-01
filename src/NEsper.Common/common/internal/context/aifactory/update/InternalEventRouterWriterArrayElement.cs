///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;

namespace com.espertech.esper.common.@internal.context.aifactory.update
{
    public class InternalEventRouterWriterArrayElement : InternalEventRouterWriter
    {
        public ExprEvaluator IndexExpression { get; set; }

        public ExprEvaluator RhsExpression { get; set; }

        public TypeWidener TypeWidener { get; set; }

        public string PropertyName { get; set; }
    }
} // end of namespace