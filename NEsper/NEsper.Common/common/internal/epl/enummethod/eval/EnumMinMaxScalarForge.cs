///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.enummethod.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionRelational.
    CodegenRelational;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval
{
    public class EnumMinMaxScalarForge : EnumForgeBase,
        EnumForge,
        EnumEval
    {
        private readonly bool max;
        private readonly EPType resultType;

        public EnumMinMaxScalarForge(
            int streamCountIncoming,
            bool max,
            EPType resultType)
            : base(streamCountIncoming)
        {
            this.max = max;
            this.resultType = resultType;
        }

        public override EnumEval EnumEvaluator => this;

        public override CodegenExpression Codegen(
            EnumForgeCodegenParams args,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var innerTypeBoxed = Boxing.GetBoxedType(EPTypeHelper.GetCodegenReturnType(resultType));

            var block = codegenMethodScope
                .MakeChild(innerTypeBoxed, typeof(EnumMinMaxEventsForgeEval), codegenClassScope)
                .AddParam(EnumForgeCodegenNames.PARAMS)
                .Block
                .DeclareVar(innerTypeBoxed, "minKey", ConstantNull());

            var forEach = block.ForEach(typeof(object), "value", EnumForgeCodegenNames.REF_ENUMCOLL)
                .IfRefNull("value")
                .BlockContinue();

            forEach.IfCondition(EqualsNull(Ref("minKey")))
                .AssignRef("minKey", Cast(innerTypeBoxed, Ref("value")))
                .IfElse()
                .IfCondition(
                    Relational(ExprDotMethod(Unbox(Ref("minKey"), innerTypeBoxed), "CompareTo", Ref("value")), max ? LT : GT, Constant(0)))
                .AssignRef("minKey", Cast(innerTypeBoxed, Ref("value")));

            var method = block.MethodReturn(Ref("minKey"));
            return LocalMethod(method, args.Expressions);
        }

        public object EvaluateEnumMethod(
            EventBean[] eventsLambda,
            ICollection<object> enumcoll,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            IComparable minKey = null;

            foreach (var next in enumcoll) {
                var comparable = next;
                if (comparable == null) {
                    continue;
                }

                if (minKey == null) {
                    minKey = (IComparable) comparable;
                }
                else {
                    if (max) {
                        if (minKey.CompareTo(comparable) < 0) {
                            minKey = (IComparable) comparable;
                        }
                    }
                    else {
                        if (minKey.CompareTo(comparable) > 0) {
                            minKey = (IComparable) comparable;
                        }
                    }
                }
            }

            return minKey;
        }
    }
} // end of namespace