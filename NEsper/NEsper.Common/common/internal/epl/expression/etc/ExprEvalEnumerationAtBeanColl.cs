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
using com.espertech.esper.common.@internal.epl.resultset.@select.typable;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.etc
{
    public class ExprEvalEnumerationAtBeanColl : ExprForge, SelectExprProcessorTypableForge
    {
        private readonly ExprEnumerationForge _enumerationForge;
        private readonly EventType _eventTypeColl;

        public ExprEvalEnumerationAtBeanColl(
            ExprEnumerationForge enumerationForge,
            EventType eventTypeColl)
        {
            this._enumerationForge = enumerationForge;
            this._eventTypeColl = eventTypeColl;
        }

        public ExprEvaluator ExprEvaluator {
            get { throw new IllegalStateException("Evaluator not available"); }
        }

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            CodegenMethod methodNode = codegenMethodScope.MakeChild(
                typeof(EventBean[]),
                this.GetType(),
                codegenClassScope);
            methodNode.Block
                .DeclareVar<FlexCollection>(
                    "result",
                    FlexWrap(_enumerationForge.EvaluateGetROCollectionEventsCodegen(methodNode, exprSymbol, codegenClassScope)))
                .IfNullReturnNull(Ref("result"))
                .MethodReturn(ExprDotMethod(ExprDotName(Ref("result"), "EventBeanCollection"), "ToArray"));
            return LocalMethod(methodNode);
        }

        public ExprForgeConstantType ForgeConstantType {
            get => ExprForgeConstantType.NONCONST;
        }

        public Type EvaluationType {
            get => typeof(EventBean[]);
        }

        public Type UnderlyingEvaluationType {
            get => TypeHelper.GetArrayType(_eventTypeColl.UnderlyingType);
        }

        public ExprNodeRenderable ExprForgeRenderable {
            get => _enumerationForge.EnumForgeRenderable;
        }
    }
} // end of namespace