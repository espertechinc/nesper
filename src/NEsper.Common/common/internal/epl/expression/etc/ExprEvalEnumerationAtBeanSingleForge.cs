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
using com.espertech.esper.common.@internal.epl.resultset.select.typable;

namespace com.espertech.esper.common.@internal.epl.expression.etc
{
    public class ExprEvalEnumerationAtBeanSingleForge : ExprForge,
        SelectExprProcessorTypableForge
    {
        private readonly ExprEnumerationForge _enumerationForge;
        private readonly EventType _eventTypeSingle;

        public ExprEvalEnumerationAtBeanSingleForge(
            ExprEnumerationForge enumerationForge,
            EventType eventTypeSingle)
        {
            _enumerationForge = enumerationForge;
            _eventTypeSingle = eventTypeSingle;
        }

        public ExprEvaluator ExprEvaluator => throw ExprNodeUtilityMake.MakeUnsupportedCompileTime();

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return _enumerationForge.EvaluateGetEventBeanCodegen(codegenMethodScope, exprSymbol, codegenClassScope);
        }


        public Type EvaluationType => typeof(EventBean);

        public Type UnderlyingEvaluationType => _eventTypeSingle.UnderlyingType;

        public ExprNodeRenderable ExprForgeRenderable => _enumerationForge.EnumForgeRenderable;

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;
    }
} // end of namespace