///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
    public class DTLocalBeanIntervalNoEndTSForge : DTLocalForge
    {
        internal readonly EventPropertyGetterSPI getter;
        internal readonly Type getterResultType;
        internal readonly DTLocalForge inner;
        internal readonly Type returnType;

        public DTLocalBeanIntervalNoEndTSForge(
            EventPropertyGetterSPI getter,
            Type getterResultType,
            DTLocalForge inner,
            Type returnType)
        {
            this.getter = getter;
            this.getterResultType = getterResultType;
            this.inner = inner;
            this.returnType = returnType;
        }

        public DTLocalEvaluator DTEvaluator => new DTLocalBeanIntervalNoEndTSEval(getter, inner.DTEvaluator);

        public CodegenExpression Codegen(
            CodegenExpression inner,
            Type innerType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return DTLocalBeanIntervalNoEndTSEval.Codegen(
                this,
                inner,
                innerType,
                codegenMethodScope,
                exprSymbol,
                codegenClassScope);
        }
    }
} // end of namespace