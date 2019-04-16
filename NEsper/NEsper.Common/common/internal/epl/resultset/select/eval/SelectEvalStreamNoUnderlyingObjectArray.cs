///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.resultset.@select.core;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.resultset.select.eval
{
    public class SelectEvalStreamNoUnderlyingObjectArray : SelectEvalStreamBaseObjectArray
    {
        public SelectEvalStreamNoUnderlyingObjectArray(
            SelectExprForgeContext context,
            EventType resultEventType,
            IList<SelectClauseStreamCompiledSpec> namedStreams,
            bool usingWildcard)
            : base(context, resultEventType, namedStreams, usingWildcard)

        {
        }

        protected override CodegenExpression ProcessSpecificCodegen(
            CodegenExpression resultEventType,
            CodegenExpression eventBeanFactory,
            CodegenExpressionRef props,
            CodegenClassScope codegenClassScope)
        {
            return ExprDotMethod(eventBeanFactory, "adapterForTypedObjectArray", props, resultEventType);
        }
    }
} // end of namespace