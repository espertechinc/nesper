///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.etc
{
    public class ExprEvalByGetterFragment : ExprForge,
        ExprNodeRenderable
    {
        private readonly FragmentEventType fragmentType;

        public ExprEvalByGetterFragment(
            int streamNum,
            EventPropertyGetterSPI getter,
            Type returnType,
            FragmentEventType fragmentType)
        {
            StreamNum = streamNum;
            Getter = getter;
            EvaluationType = returnType;
            this.fragmentType = fragmentType;
        }

        public EventPropertyGetterSPI Getter { get; }

        public int StreamNum { get; }

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var result = fragmentType.IsIndexed ? typeof(EventBean[]) : typeof(EventBean);
            var methodNode = codegenMethodScope.MakeChild(result, typeof(ExprEvalByGetterFragment), codegenClassScope);

            var refEPS = exprSymbol.GetAddEPS(methodNode);
            methodNode.Block
                .DeclareVar<EventBean>("event", ArrayAtIndex(refEPS, Constant(StreamNum)))
                .IfRefNullReturnNull("event")
                .MethodReturn(
                    Cast(result, Getter.EventBeanFragmentCodegen(Ref("event"), methodNode, codegenClassScope)));
            return LocalMethod(methodNode);
        }

        public Type EvaluationType { get; }

        public ExprNodeRenderable ExprForgeRenderable => this;

        public ExprEvaluator ExprEvaluator => throw new IllegalStateException("Evaluator not available");

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

        public void ToEPL(
            TextWriter writer,
            ExprPrecedenceEnum parentPrecedence)
        {
            writer.Write(GetType().Name);
        }
    }
} // end of namespace