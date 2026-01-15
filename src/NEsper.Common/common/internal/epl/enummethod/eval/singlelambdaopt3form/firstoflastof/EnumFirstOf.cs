///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.enummethod.codegen;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.firstoflastof {
    public class EnumFirstOf : EnumForgeBasePlain,
        EnumForge,
        EnumEval {
        private readonly EPChainableType _resultType;

        public EnumFirstOf(
            int streamCountIncoming,
            EPChainableType resultType) : base(streamCountIncoming)
        {
            _resultType = resultType;
        }

        public override EnumEval EnumEvaluator => this;

        public object EvaluateEnumMethod(
            EventBean[] eventsLambda,
            ICollection<object> enumcoll,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            if (enumcoll == null || enumcoll.IsEmpty()) {
                return null;
            }

            return enumcoll.First();
        }

        public override CodegenExpression Codegen(
            EnumForgeCodegenParams args,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return StaticMethod(typeof(EnumFirstOf), "FirstValue", args.Expressions);
        }

        public static T FirstValue<T>(
            EventBean[] eventsPerStream,
            ICollection<T> enumcoll,
            bool isNewData,
            ExprEvaluatorContext exprEvalCtx)
        {
            if ((enumcoll == null || enumcoll.IsEmpty())) {
                return default;
            }

            return enumcoll.FirstOrDefault();
        }
    }
} // end of namespace