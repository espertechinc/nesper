///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.dot.core;
using com.espertech.esper.common.@internal.rettype;

namespace com.espertech.esper.common.@internal.epl.expression.dot.inner
{
    public class InnerDotEnumerableEventBeanForge : ExprDotEvalRootChildInnerForge
    {
        internal readonly ExprEnumerationForge rootLambdaForge;
        internal readonly EventType eventType;

        public InnerDotEnumerableEventBeanForge(
            ExprEnumerationForge rootLambdaForge,
            EventType eventType)
        {
            this.rootLambdaForge = rootLambdaForge;
            this.eventType = eventType;
        }

        public ExprDotEvalRootChildInnerEval InnerEvaluator {
            get => new InnerDotEnumerableEventBeanEval(rootLambdaForge.ExprEvaluatorEnumeration);
        }

        public CodegenExpression CodegenEvaluate(
            CodegenMethod parentMethod,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return rootLambdaForge.EvaluateGetEventBeanCodegen(parentMethod, exprSymbol, codegenClassScope);
        }

        public CodegenExpression EvaluateGetROCollectionEventsCodegen(
            CodegenMethod parentMethod,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return rootLambdaForge.EvaluateGetROCollectionEventsCodegen(parentMethod, exprSymbol, codegenClassScope);
        }

        public CodegenExpression EvaluateGetROCollectionScalarCodegen(
            CodegenMethod parentMethod,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return rootLambdaForge.EvaluateGetROCollectionScalarCodegen(parentMethod, exprSymbol, codegenClassScope);
        }

        public CodegenExpression EvaluateGetEventBeanCodegen(
            CodegenMethod parentMethod,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return rootLambdaForge.EvaluateGetEventBeanCodegen(parentMethod, exprSymbol, codegenClassScope);
        }

        public EventType EventTypeCollection {
            get => null;
        }

        public Type ComponentTypeCollection {
            get => null;
        }

        public EventType EventTypeSingle {
            get => eventType;
        }

        public EPType TypeInfo {
            get => EPTypeHelper.SingleEvent(eventType);
        }
    }
} // end of namespace