///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.collection;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.expression.etc
{
    public class ExprEvalStreamNumEnumCollForge : ExprForge
    {
        private readonly ExprEnumerationForge _enumeration;
        private readonly Type _evaluationType;

        public ExprEvalStreamNumEnumCollForge(ExprEnumerationForge enumeration)
        {
            _enumeration = enumeration;

            // NOTE: the forge knows the type that needs to be rendered.  In Java, they use the
            //   generic "Collection" type which allows them to not specify which type of collection
            //   they are using.  In C# we have strong type checking which means we need to know.
            //   Unfortunately, this data is only known in the ExprForge.  Revisit this.

            _evaluationType = typeof(FlexCollection);
        }

        public ExprEvaluator ExprEvaluator => throw new UnsupportedOperationException("Not available at compile time");

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return _enumeration.EvaluateGetROCollectionEventsCodegen(codegenMethodScope, exprSymbol, codegenClassScope);
        }

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

        public Type EvaluationType => _evaluationType;

        public ExprNodeRenderable ExprForgeRenderable => _enumeration.EnumForgeRenderable;
    }
} // end of namespace