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

namespace com.espertech.esper.common.@internal.epl.expression.etc
{
    public class ExprEvalEnumerationAtBeanSingleForge : ExprForge
    {
        internal readonly ExprEnumerationForge enumerationForge;
        private readonly EventType eventTypeSingle;

        public ExprEvalEnumerationAtBeanSingleForge(
            ExprEnumerationForge enumerationForge,
            EventType eventTypeSingle)
        {
            this.enumerationForge = enumerationForge;
            this.eventTypeSingle = eventTypeSingle;
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
            return enumerationForge.EvaluateGetEventBeanCodegen(codegenMethodScope, exprSymbol, codegenClassScope);
        }

        public Type EvaluationType {
            get => eventTypeSingle.UnderlyingType;
        }

        public ExprNodeRenderable ExprForgeRenderable {
            get => enumerationForge.EnumForgeRenderable;
        }

        public ExprForgeConstantType ForgeConstantType {
            get => ExprForgeConstantType.NONCONST;
        }
    }
} // end of namespace