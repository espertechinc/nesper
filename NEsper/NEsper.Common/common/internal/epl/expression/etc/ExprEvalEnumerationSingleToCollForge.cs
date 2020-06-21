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
using com.espertech.esper.common.@internal.epl.resultset.@select.typable;
using com.espertech.esper.common.@internal.util;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.etc
{
    public class ExprEvalEnumerationSingleToCollForge : ExprForge, SelectExprProcessorTypableForge
    {
        private readonly ExprEnumerationForge _;
        private readonly EventType _targetType;

        public ExprEvalEnumerationSingleToCollForge(
            ExprEnumerationForge enumerationForge,
            EventType targetType)
        {
            this._ = enumerationForge;
            this._targetType = targetType;
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
            CodegenMethod methodNode = codegenMethodScope.MakeChild(
                typeof(EventBean[]),
                typeof(ExprEvalEnumerationSingleToCollForge),
                codegenClassScope);

            methodNode.Block
                .DeclareVar<EventBean>(
                    "@event",
                    _.EvaluateGetEventBeanCodegen(methodNode, exprSymbol, codegenClassScope))
                .IfRefNullReturnNull("@event")
                .DeclareVar<EventBean[]>("events", NewArrayByLength(typeof(EventBean), Constant(1)))
                .AssignArrayElement(Ref("events"), Constant(0), Ref("@event"))
                .MethodReturn(Ref("events"));
            return LocalMethod(methodNode);
        }

        public ExprForgeConstantType ForgeConstantType {
            get => ExprForgeConstantType.NONCONST;
        }

        public Type EvaluationType {
            get => typeof(EventBean[]);
        }

        public Type UnderlyingEvaluationType {
            get => TypeHelper.GetArrayType(_targetType.UnderlyingType);
        }

        public ExprNodeRenderable ExprForgeRenderable {
            get => _.EnumForgeRenderable;
        }
    }
} // end of namespace