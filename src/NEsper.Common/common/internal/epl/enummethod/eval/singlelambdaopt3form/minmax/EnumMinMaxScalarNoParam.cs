///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.enummethod.codegen;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionRelational.
    CodegenRelational;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.minmax
{
    public class EnumMinMaxScalarNoParam : EnumForgeBasePlain,
        EnumForge,
        EnumEval
    {
        private readonly bool _max;
        private readonly EPChainableType _resultType;

        public EnumMinMaxScalarNoParam(
            int streamCountIncoming,
            bool max,
            EPChainableType resultType) : base(streamCountIncoming)
        {
            _max = max;
            _resultType = resultType;
        }

        public override EnumEval EnumEvaluator => this;

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
                    minKey = (IComparable)comparable;
                }
                else {
                    if (_max) {
                        if (minKey.CompareTo(comparable) < 0) {
                            minKey = (IComparable)comparable;
                        }
                    }
                    else {
                        if (minKey.CompareTo(comparable) > 0) {
                            minKey = (IComparable)comparable;
                        }
                    }
                }
            }

            return minKey;
        }

        public override CodegenExpression Codegen(
            EnumForgeCodegenParams args,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var innerType = _resultType.GetCodegenReturnType();
            var innerTypeBoxed = innerType.GetBoxedType();
            //var innerTypeCollection = typeof(ICollection<>).MakeGenericType(innerTypeBoxed);

            var block = codegenMethodScope
                .MakeChild(innerTypeBoxed, typeof(EnumMinMaxScalarNoParam), codegenClassScope)
                .AddParam(ExprForgeCodegenNames.FP_EPS)
                .AddParam(args.EnumcollType, EnumForgeCodegenNames.REF_ENUMCOLL.Ref)
                .AddParam(ExprForgeCodegenNames.FP_ISNEWDATA)
                .AddParam(ExprForgeCodegenNames.FP_EXPREVALCONTEXT)
                .Block
                .CommentFullLine(MethodBase.GetCurrentMethod()!.DeclaringType!.FullName + "." + MethodBase.GetCurrentMethod()!.Name)
                .DeclareVar(innerTypeBoxed, "minKey", ConstantNull())
                .DeclareVar(args.EnumcollType, "coll", EnumForgeCodegenNames.REF_ENUMCOLL);

            var forEach = block
                .ForEach(innerTypeBoxed, "value", Ref("coll"))
                .IfRefNull("value")
                .BlockContinue();

            var compareTo =
                StaticMethod(
                    typeof(SmartCompare),
                    "Compare",
                    Ref("minKey"),
                    Ref("value"));

            forEach
                .IfCondition(EqualsNull(Ref("minKey")))
                .AssignRef("minKey", Ref("value"))
                .IfElse()
                .IfCondition(Relational(compareTo, _max ? LT : GT, Constant(0)))
                .AssignRef("minKey", Ref("value"));

            var method = block.MethodReturn(Ref("minKey"));
            return LocalMethod(method, args.Expressions);
        }
    }
} // end of namespace