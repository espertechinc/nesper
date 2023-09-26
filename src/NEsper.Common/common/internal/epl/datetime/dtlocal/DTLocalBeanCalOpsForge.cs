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
    public class DTLocalBeanCalOpsForge : DTLocalForge
    {
        internal readonly EventPropertyGetterSPI getter;
        internal readonly Type getterReturnType;
        internal readonly DTLocalForge inner;
        internal readonly Type innerReturnType;

        public DTLocalBeanCalOpsForge(
            EventPropertyGetterSPI getter,
            Type getterReturnType,
            DTLocalForge inner,
            Type innerReturnType)
        {
            this.getter = getter;
            this.getterReturnType = getterReturnType;
            this.inner = inner;
            this.innerReturnType = innerReturnType;
        }

        public DTLocalEvaluator DTEvaluator => new DTLocalBeanCalOpsEval(this, inner.DTEvaluator);

        public CodegenExpression Codegen(
            CodegenExpression inner,
            Type innerType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return DTLocalBeanCalOpsEval.Codegen(
                this,
                inner,
                innerType,
                codegenMethodScope,
                exprSymbol,
                codegenClassScope);
        }
    }
} // end of namespace