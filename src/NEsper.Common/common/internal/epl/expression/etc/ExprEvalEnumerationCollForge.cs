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
using com.espertech.esper.common.@internal.epl.resultset.select.typable;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.etc
{
    public class ExprEvalEnumerationCollForge : ExprForge,
        SelectExprProcessorTypableForge
    {
        private readonly ExprEnumerationForge _enumerationForge;
        private readonly EventType _targetType;
        private readonly bool _firstRowOnly;

        public ExprEvalEnumerationCollForge(
            ExprEnumerationForge enumerationForge,
            EventType targetType,
            bool firstRowOnly)
        {
            _enumerationForge = enumerationForge;
            _targetType = targetType;
            _firstRowOnly = firstRowOnly;
        }

        public ExprEvaluator ExprEvaluator => throw ExprNodeUtilityMake.MakeUnsupportedCompileTime();

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            if (_firstRowOnly)
            {
                var firstMethodNode = codegenMethodScope
                    .MakeChild(typeof(EventBean), typeof(ExprEvalEnumerationCollForge), codegenClassScope);
                firstMethodNode.Block
                    .DeclareVar<ICollection<EventBean>>(
                        "events",
                        _enumerationForge.EvaluateGetROCollectionEventsCodegen(
                            firstMethodNode,
                            exprSymbol,
                            codegenClassScope))
                    .IfRefNullReturnNull("events")
                    .IfCondition(EqualsIdentity(ExprDotName(Ref("events"), "Count"), Constant(0)))
                    .BlockReturn(ConstantNull())
                    .MethodReturn(StaticMethod(typeof(EventBeanUtility), "GetNonemptyFirstEvent", Ref("events")));
                return LocalMethod(firstMethodNode);
            }

            var methodNode = codegenMethodScope.MakeChild(
                typeof(EventBean[]),
                typeof(ExprEvalEnumerationCollForge),
                codegenClassScope);
            methodNode.Block
                .DeclareVar<ICollection<EventBean>>(
                    "events",
                    _enumerationForge.EvaluateGetROCollectionEventsCodegen(
                        methodNode,
                        exprSymbol,
                        codegenClassScope))
                .IfRefNullReturnNull("events")
                .MethodReturn(UnwrapIntoArray<EventBean>(Ref("events")));
            return LocalMethod(methodNode);
        }

        public Type UnderlyingEvaluationType =>
            _firstRowOnly
                ? _targetType.UnderlyingType
                : TypeHelper.GetArrayType(_targetType.UnderlyingType);

        public Type EvaluationType =>
            _firstRowOnly
                ? typeof(EventBean)
                : typeof(EventBean[]);

        public ExprNodeRenderable ExprForgeRenderable => _enumerationForge.EnumForgeRenderable;

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;
    }
} // end of namespace