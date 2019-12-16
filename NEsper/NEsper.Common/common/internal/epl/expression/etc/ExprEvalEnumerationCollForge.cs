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
using com.espertech.esper.common.client.collection;
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

        public ExprEvalEnumerationCollForge(
            ExprEnumerationForge enumerationForge,
            EventType targetType,
            bool firstRowOnly)
        {
            this.enumerationForge = enumerationForge;
            this.targetType = targetType;
            this.firstRowOnly = firstRowOnly;
        }

        public ExprEvaluator ExprEvaluator {
            get { throw ExprNodeUtilityMake.MakeUnsupportedCompileTime(); }
        }

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            if (firstRowOnly) {
                CodegenMethod firstMethodNode = codegenMethodScope
                    .MakeChild(typeof(EventBean), typeof(ExprEvalEnumerationCollForge), codegenClassScope);
                firstMethodNode.Block
                    .DeclareVar<FlexCollection>(
                        "events",
                        enumerationForge.EvaluateGetROCollectionEventsCodegen(
                            firstMethodNode,
                            exprSymbol,
                            codegenClassScope))
                    .IfRefNullReturnNull("events")
                    .IfCondition(EqualsIdentity(ExprDotName(Ref("events"), "Count"), Constant(0)))
                    .BlockReturn(ConstantNull())
                    .MethodReturn(StaticMethod(typeof(EventBeanUtility), "GetNonemptyFirstEvent", Ref("events")));
                return LocalMethod(firstMethodNode);
            }

            CodegenMethod methodNode = codegenMethodScope.MakeChild(
                typeof(EventBean[]),
                typeof(ExprEvalEnumerationCollForge),
                codegenClassScope);
            methodNode.Block
                .DeclareVar<FlexCollection>(
                    "events",
                    FlexWrap(enumerationForge.EvaluateGetROCollectionEventsCodegen(methodNode, exprSymbol, codegenClassScope)))
                .IfRefNullReturnNull("events")
                .MethodReturn(ExprDotMethod(ExprDotName(Ref("events"), "EventBeanCollection"), "ToArray"));
            return LocalMethod(methodNode);
        }

        public Type EvaluationType {
            get {
                if (firstRowOnly) {
                    return targetType.UnderlyingType;
                }

                return TypeHelper.GetArrayType(targetType.UnderlyingType);
            }
        }

        public ExprNodeRenderable ExprForgeRenderable {
            get => enumerationForge.EnumForgeRenderable;
        }

        public ExprForgeConstantType ForgeConstantType {
            get => ExprForgeConstantType.NONCONST;
        }
    }
} // end of namespace