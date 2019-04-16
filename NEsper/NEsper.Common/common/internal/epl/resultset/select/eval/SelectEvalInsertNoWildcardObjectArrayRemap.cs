///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.resultset.select.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.resultset.select.eval
{
    public class SelectEvalInsertNoWildcardObjectArrayRemap : SelectExprProcessorForge
    {
        internal readonly SelectExprForgeContext context;
        internal readonly EventType resultEventType;
        internal readonly int[] remapped;

        public SelectEvalInsertNoWildcardObjectArrayRemap(
            SelectExprForgeContext context,
            EventType resultEventType,
            int[] remapped)
        {
            this.context = context;
            this.resultEventType = resultEventType;
            this.remapped = remapped;
        }

        public EventType ResultEventType {
            get => resultEventType;
        }

        public CodegenMethod ProcessCodegen(
            CodegenExpression resultEventTypeExpr,
            CodegenExpression eventBeanFactory,
            CodegenMethodScope codegenMethodScope,
            SelectExprProcessorCodegenSymbol selectSymbol,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return SelectEvalInsertNoWildcardObjectArrayRemapWWiden.ProcessCodegen(
                resultEventTypeExpr, eventBeanFactory, codegenMethodScope, exprSymbol, codegenClassScope, context.ExprForges,
                resultEventType.PropertyNames, remapped, null);
        }
    }
} // end of namespace