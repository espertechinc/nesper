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
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionRelational.
    CodegenRelational;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval
{
    public class EnumDistinctScalarForge : EnumForgeBase,
        EnumForge,
        EnumEval
    {
        public EnumDistinctScalarForge(int streamCountIncoming)
            : base(streamCountIncoming)
        {
        }

        public object EvaluateEnumMethod(
            EventBean[] eventsLambda,
            ICollection<object> enumcoll,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            if (enumcoll.Count <= 1) {
                return enumcoll;
            }

            if (enumcoll is ISet<object>) {
                return enumcoll;
            }

            return new LinkedHashSet<object>(enumcoll);
        }

        public override EnumEval EnumEvaluator => this;

        public override CodegenExpression Codegen(
            EnumForgeCodegenParams args,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var method = codegenMethodScope
                .MakeChild(typeof(ICollection<object>), typeof(EnumDistinctScalarForge), codegenClassScope)
                .AddParam(EnumForgeCodegenNames.PARAMS)
                .Block
                .IfCondition(Relational(ExprDotName(EnumForgeCodegenNames.REF_ENUMCOLL, "Count"), LE, Constant(1)))
                .BlockReturn(EnumForgeCodegenNames.REF_ENUMCOLL)
                .IfCondition(InstanceOf(Ref("enumcoll"), typeof(ISet<object>)))
                .BlockReturn(EnumForgeCodegenNames.REF_ENUMCOLL)
                .MethodReturn(NewInstance<LinkedHashSet<object>>(EnumForgeCodegenNames.REF_ENUMCOLL));
            return LocalMethod(method, args.Expressions);
        }
    }
} // end of namespace