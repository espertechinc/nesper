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
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.dot.core;
using com.espertech.esper.common.@internal.util;

namespace com.espertech.esper.common.@internal.epl.expression.dot.inner
{
    public class InnerDotArrPrimitiveToCollEval : ExprDotEvalRootChildInnerEval
    {
        private readonly ExprEvaluator rootEvaluator;

        public InnerDotArrPrimitiveToCollEval(ExprEvaluator rootEvaluator)
        {
            this.rootEvaluator = rootEvaluator;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            object array = rootEvaluator.Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
            return CollectionUtil.ArrayToCollectionAllowNull<object>(array);
        }

        public static CodegenExpression Codegen(
            InnerDotArrPrimitiveToCollForge forge,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            Type evaluationType = forge.rootForge.EvaluationType;
            return CollectionUtil.ArrayToCollectionAllowNullCodegen(
                codegenMethodScope, evaluationType,
                forge.rootForge.EvaluateCodegen(evaluationType, codegenMethodScope, exprSymbol, codegenClassScope), codegenClassScope);
        }

        public ICollection<EventBean> EvaluateGetROCollectionEvents(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return null;
        }

        public ICollection<object> EvaluateGetROCollectionScalar(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return null;
        }

        public EventBean EvaluateGetEventBean(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return null;
        }
    }
} // end of namespace