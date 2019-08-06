///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.enummethod.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval
{
    public class EnumReverseForge : EnumEval,
        EnumForge
    {
        public EnumReverseForge(int numStreams)
        {
            StreamNumSize = numStreams;
        }

        public object EvaluateEnumMethod(
            EventBean[] eventsLambda,
            ICollection<object> enumcoll,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            if (enumcoll.IsEmpty()) {
                return enumcoll;
            }

            var result = new List<object>(enumcoll);
            result.Reverse();
            return result;
        }

        public virtual EnumEval EnumEvaluator => this;

        public int StreamNumSize { get; }

        public CodegenExpression Codegen(
            EnumForgeCodegenParams args,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var method = codegenMethodScope
                .MakeChild(typeof(ICollection<object>), typeof(EnumReverseForge), codegenClassScope)
                .AddParam(EnumForgeCodegenNames.PARAMS)
                .Block
                .IfCondition(ExprDotMethod(EnumForgeCodegenNames.REF_ENUMCOLL, "IsEmpty"))
                .BlockReturn(EnumForgeCodegenNames.REF_ENUMCOLL)
                .DeclareVar<List<object>>(
                    "result",
                    NewInstance<List<object>>(EnumForgeCodegenNames.REF_ENUMCOLL))
                .StaticMethod(typeof(Collections), "Reverse", Ref("result"))
                .MethodReturn(Ref("result"));
            return LocalMethod(method, args.Expressions);
        }
    }
} // end of namespace