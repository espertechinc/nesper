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
    public class ExprEvalStreamNumEnumSingleForge : ExprForge
    {
        private readonly ExprEnumerationForge _enumeration;

        public ExprEvalStreamNumEnumSingleForge(ExprEnumerationForge enumeration)
        {
            _enumeration = enumeration;
        }

        public ExprEvaluator ExprEvaluator => throw ExprNodeUtilityMake.MakeUnsupportedCompileTime();

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return _enumeration.EvaluateGetEventBeanCodegen(codegenMethodScope, exprSymbol, codegenClassScope);
        }

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

        public Type EvaluationType => typeof(EventBean);

        public ExprNodeRenderable ExprForgeRenderable => _enumeration.EnumForgeRenderable;
    }
} // end of namespace