///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.resultset.select.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.resultset.select.eval
{
    public class SelectEvalInsertNoWildcardSingleColCoercionBean : SelectEvalBaseFirstProp
    {
        public SelectEvalInsertNoWildcardSingleColCoercionBean(
            SelectExprForgeContext selectExprForgeContext,
            EventType resultEventType)
            : base(selectExprForgeContext, resultEventType)

        {
        }

        protected override CodegenExpression ProcessFirstColCodegen(
            Type evaluationType,
            CodegenExpression expression,
            CodegenExpression resultEventType,
            CodegenExpression eventBeanFactory,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return ExprDotMethod(eventBeanFactory, "AdapterForTypedObject", expression, resultEventType);
        }
    }
} // end of namespace