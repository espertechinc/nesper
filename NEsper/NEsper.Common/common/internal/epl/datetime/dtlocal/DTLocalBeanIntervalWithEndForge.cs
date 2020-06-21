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
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.@event.core;

namespace com.espertech.esper.common.@internal.epl.datetime.dtlocal
{
    public class DTLocalBeanIntervalWithEndForge : DTLocalForge
    {
        internal readonly EventPropertyGetterSPI getterStartTimestamp;
        internal readonly Type getterStartReturnType;
        internal readonly EventPropertyGetterSPI getterEndTimestamp;
        internal readonly Type getterEndReturnType;
        internal readonly DTLocalForgeIntervalComp inner;

        public DTLocalBeanIntervalWithEndForge(
            EventPropertyGetterSPI getterStartTimestamp,
            Type getterStartReturnType,
            EventPropertyGetterSPI getterEndTimestamp,
            Type getterEndReturnType,
            DTLocalForgeIntervalComp inner)
        {
            this.getterStartTimestamp = getterStartTimestamp;
            this.getterStartReturnType = getterStartReturnType;
            this.getterEndTimestamp = getterEndTimestamp;
            this.getterEndReturnType = getterEndReturnType;
            this.inner = inner;
        }

        public DTLocalEvaluator DTEvaluator {
            get => new DTLocalBeanIntervalWithEndEval(
                getterStartTimestamp,
                getterEndTimestamp,
                inner.MakeEvaluatorComp());
        }

        public CodegenExpression Codegen(
            CodegenExpression inner,
            Type innerType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return DTLocalBeanIntervalWithEndEval.Codegen(
                this,
                inner,
                codegenMethodScope,
                exprSymbol,
                codegenClassScope);
        }
    }
} // end of namespace