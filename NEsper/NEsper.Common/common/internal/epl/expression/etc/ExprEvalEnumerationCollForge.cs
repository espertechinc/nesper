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
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.etc
{
    public class ExprEvalEnumerationCollForge : ExprForge
    {
        internal readonly ExprEnumerationForge enumerationForge;
        private readonly EventType targetType;
        private readonly bool firstRowOnly;

        public ExprEvalEnumerationCollForge(ExprEnumerationForge enumerationForge, EventType targetType, bool firstRowOnly)
        {
            this.enumerationForge = enumerationForge;
            this.targetType = targetType;
            this.firstRowOnly = firstRowOnly;
        }

        public ExprEvaluator ExprEvaluator
        {
            get { throw ExprNodeUtilityMake.MakeUnsupportedCompileTime(); }
        }

        public CodegenExpression EvaluateCodegen(Type requiredType, CodegenMethodScope codegenMethodScope, ExprForgeCodegenSymbol exprSymbol, CodegenClassScope codegenClassScope)
        {
            if (firstRowOnly)
            {
                CodegenMethod methodNode = codegenMethodScope.MakeChild(typeof(EventBean), typeof(ExprEvalEnumerationCollForge), codegenClassScope);
                methodNode.Block
                        .DeclareVar(typeof(ICollection<object>), typeof(EventBean), "events", enumerationForge.EvaluateGetROCollectionEventsCodegen(methodNode, exprSymbol, codegenClassScope))
                        .IfRefNullReturnNull("events")
                        .IfCondition(EqualsIdentity(ExprDotMethod(@Ref("events"), "size"), Constant(0)))
                        .BlockReturn(ConstantNull())
                        .MethodReturn(StaticMethod(typeof(EventBeanUtility), "getNonemptyFirstEvent", @Ref("events")));
                return LocalMethod(methodNode);
            }

            CodegenMethod methodNode = codegenMethodScope.MakeChild(typeof(EventBean[]), typeof(ExprEvalEnumerationCollForge), codegenClassScope);
            methodNode.Block
                    .DeclareVar(typeof(ICollection<object>), typeof(EventBean), "events", enumerationForge.EvaluateGetROCollectionEventsCodegen(methodNode, exprSymbol, codegenClassScope))
                    .IfRefNullReturnNull("events")
                    .MethodReturn(Cast(typeof(EventBean[]), ExprDotMethod(@Ref("events"), "toArray", NewArrayByLength(typeof(EventBean), ExprDotMethod(@Ref("events"), "size")))));
            return LocalMethod(methodNode);
        }

        public Type EvaluationType
        {
            get
            {
                if (firstRowOnly)
                {
                    return targetType.UnderlyingType;
                }

                return TypeHelper.GetArrayType(targetType.UnderlyingType);
            }
        }

        public ExprNodeRenderable ForgeRenderable
        {
            get => enumerationForge.ForgeRenderable;
        }

        public ExprForgeConstantType ForgeConstantType
        {
            get => ExprForgeConstantType.NONCONST;
        }
    }
} // end of namespace