///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.resultset.select.core;
using com.espertech.esper.common.@internal.@event.core;

namespace com.espertech.esper.common.@internal.epl.resultset.select.eval
{
    public class SelectEvalInsertNoWildcardSingleColCoercionAvroWrap : SelectEvalBaseFirstPropFromWrap
    {
        public SelectEvalInsertNoWildcardSingleColCoercionAvroWrap(
            SelectExprForgeContext selectExprForgeContext,
            WrapperEventType wrapper)
            : base(selectExprForgeContext, wrapper)

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
            return SelectEvalInsertNoWildcardSingleColCoercionMapWrap.ProcessFirstColCodegen(
                expression,
                eventBeanFactory,
                codegenClassScope,
                wrapper,
                "AdapterForTypedAvro",
                typeof(object));
        }
    }
} // end of namespace