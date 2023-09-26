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
using com.espertech.esper.common.@internal.rettype;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.dot.inner
{
    public class InnerDotEnumerableScalarCollectionForge : ExprDotEvalRootChildInnerForge
    {
        internal readonly ExprEnumerationForge rootLambdaForge;
        internal readonly Type componentType;

        public InnerDotEnumerableScalarCollectionForge(
            ExprEnumerationForge rootLambdaForge,
            Type componentType)
        {
            this.rootLambdaForge = rootLambdaForge;
            this.componentType = componentType;
        }

        public ExprDotEvalRootChildInnerEval InnerEvaluator =>
            new InnerDotEnumerableScalarCollectionEval(rootLambdaForge.ExprEvaluatorEnumeration);

        public CodegenExpression CodegenEvaluate(
            CodegenMethod parentMethod,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var collectionScalarCodegen =
                rootLambdaForge.EvaluateGetROCollectionScalarCodegen(parentMethod, exprSymbol, codegenClassScope);
            var collectionType = typeof(ICollection<>).MakeGenericType(componentType);
            return FlexCast(collectionType, collectionScalarCodegen);
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
            return ConstantNull();
        }

        public EventType EventTypeCollection => null;

        public Type ComponentTypeCollection => componentType;

        public EventType EventTypeSingle => null;

        public EPChainableType TypeInfo =>
            EPChainableTypeHelper.CollectionOfSingleValue(
                componentType);
    }
} // end of namespace