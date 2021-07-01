///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.historical.method.poll
{
    public class MethodTargetStrategyStaticMethodForge : MethodTargetStrategyForge
    {
        private readonly Type clazz;
        private readonly MethodInfo reflectionMethod;

        public MethodTargetStrategyStaticMethodForge(
            Type clazz,
            MethodInfo reflectionMethod)
        {
            this.clazz = clazz;
            this.reflectionMethod = reflectionMethod;
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            CodegenMethod method = parent.MakeChild(
                typeof(MethodTargetStrategyStaticMethod),
                this.GetType(),
                classScope);
            method.Block
                .DeclareVar<MethodTargetStrategyStaticMethod>(
                    "target",
                    NewInstance(typeof(MethodTargetStrategyStaticMethod)))
                .SetProperty(Ref("target"), "Clazz", Constant(clazz))
                .SetProperty(Ref("target"), "MethodName", Constant(reflectionMethod.Name))
                .SetProperty(Ref("target"), "MethodParameters", Constant(reflectionMethod.GetParameterTypes()))
                .Expression(ExprDotMethodChain(symbols.GetAddInitSvc(method)).Add("AddReadyCallback", Ref("target")))
                .MethodReturn(Ref("target"));
            return LocalMethod(method);
        }
    }
} // end of namespace