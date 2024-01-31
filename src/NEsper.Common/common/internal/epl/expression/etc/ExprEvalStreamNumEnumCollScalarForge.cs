///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.expression.etc
{
    public class ExprEvalStreamNumEnumCollScalarForge : ExprForge
    {
        private readonly ExprEnumerationForge enumeration;

        public ExprEvalStreamNumEnumCollScalarForge(ExprEnumerationForge enumeration)
        {
            this.enumeration = enumeration;
        }

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return enumeration.EvaluateGetROCollectionEventsCodegen(codegenMethodScope, exprSymbol, codegenClassScope);
        }

        public ExprEvaluator ExprEvaluator => throw new UnsupportedOperationException("Not available at compile time");

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

        public Type EvaluationType => typeof(ICollection<EventBean>);

        public ExprNodeRenderable ExprForgeRenderable => enumeration.EnumForgeRenderable;
    }
} // end of namespace